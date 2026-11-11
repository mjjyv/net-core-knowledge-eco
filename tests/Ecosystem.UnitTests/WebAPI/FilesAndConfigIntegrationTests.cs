using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ecosystem.Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Ecosystem.UnitTests.WebAPI;

public sealed class FilesAndConfigWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"ecosystem_files_test_{Guid.NewGuid():N}.db");

    public string UploadDirectory { get; } = Path.Combine(
        Path.GetTempPath(),
        $"ecosystem_uploads_test_{Guid.NewGuid():N}");

    public string DynamicOverrideConfigPath { get; } = Path.Combine(
        Path.GetTempPath(),
        $"ecosystem_dynamic_cfg_{Guid.NewGuid():N}.json");

    public IConfigurationRoot? ConfigurationRoot { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(UploadDirectory);
        File.WriteAllText(DynamicOverrideConfigPath, """
            {
              "FileStorage": {
                "MaxFileSizeBytes": 5242880
              }
            }
            """);

        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("DatabaseProvider", "Sqlite");
        builder.UseSetting("FileStorage:UploadFolder", UploadDirectory);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddJsonFile(DynamicOverrideConfigPath, optional: false, reloadOnChange: false);
            ConfigurationRoot = configBuilder.Build();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
            {
                var dbFile = _dbPath + suffix;
                if (File.Exists(dbFile))
                {
                    try { File.Delete(dbFile); } catch { /* ignore cleanup locks */ }
                }
            }

            if (Directory.Exists(UploadDirectory))
            {
                try { Directory.Delete(UploadDirectory, recursive: true); } catch { /* ignore cleanup */ }
            }

            if (File.Exists(DynamicOverrideConfigPath))
            {
                try { File.Delete(DynamicOverrideConfigPath); } catch { /* ignore cleanup */ }
            }
        }
    }
}

public class FilesAndConfigIntegrationTests : IClassFixture<FilesAndConfigWebApplicationFactory>
{
    private static readonly byte[] ValidPngBytes =
        { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52 };

    private static readonly byte[] FakeExeBytes =
        { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };

    private readonly FilesAndConfigWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FilesAndConfigIntegrationTests(FilesAndConfigWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DevelopmentEnvironment_ExposesSwaggerJsonWithMultipartUploadSchema()
    {
        // Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var swaggerJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/v1/files/upload", swaggerJson);
        Assert.Contains("/api/v1/config/environment", swaggerJson);
    }

    [Fact]
    public async Task Upload_WhenAnonymous_Returns401UnauthorizedWithProblemDetails()
    {
        using var form = CreateSingleFileContent(ValidPngBytes, "diagram.png", "image/png");

        var response = await _client.PostAsync("/api/v1/files/upload", form);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task UploadDownloadAndDelete_EnforcesValidationAndRoleMatrix()
    {
        // Arrange
        var audienceToken = await LoginAsync("user_demo", "User@123456");
        var presenterToken = await LoginAsync("presenter_demo", "Presenter@123456");

        // 1. Public GET limits -> 200 OK
        var limitsResp = await _client.GetAsync("/api/v1/files/limits");
        Assert.Equal(HttpStatusCode.OK, limitsResp.StatusCode);

        // 2. Upload valid PNG as Audience ([Authorize] allows authenticated users) -> 201 Created
        using var uploadForm = CreateSingleFileContent(ValidPngBytes, "architecture.png", "image/png");
        var uploadResp = await SendMultipartAsync("/api/v1/files/upload", uploadForm, audienceToken);
        Assert.Equal(HttpStatusCode.Created, uploadResp.StatusCode);

        using var uploadDoc = JsonDocument.Parse(await uploadResp.Content.ReadAsStringAsync());
        var data = uploadDoc.RootElement.GetProperty("data");
        var storedFileName = data.GetProperty("storedFileName").GetString()!;
        Assert.EndsWith(".png", storedFileName);

        // 3. Public Download via API returns exact bytes + X-Content-Type-Options: nosniff
        var downloadResp = await _client.GetAsync($"/api/v1/files/{storedFileName}");
        Assert.Equal(HttpStatusCode.OK, downloadResp.StatusCode);
        Assert.Equal("image/png", downloadResp.Content.Headers.ContentType?.MediaType);
        Assert.Contains("nosniff", downloadResp.Headers.GetValues("X-Content-Type-Options"));
        var downloadedBytes = await downloadResp.Content.ReadAsByteArrayAsync();
        Assert.Equal(ValidPngBytes, downloadedBytes);

        // 4. Path Traversal download attempt -> 400 BadRequest
        var traversalResp = await _client.GetAsync("/api/v1/files/..%2Fappsettings.json");
        Assert.Equal(HttpStatusCode.BadRequest, traversalResp.StatusCode);

        // 5. Audience cannot DELETE file (RequireContentCreator) -> 403 Forbidden
        using var delReqAud = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/files/{storedFileName}");
        delReqAud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", audienceToken);
        var delRespAud = await _client.SendAsync(delReqAud);
        Assert.Equal(HttpStatusCode.Forbidden, delRespAud.StatusCode);

        // 6. Presenter CAN DELETE file -> 204 NoContent, then GET -> 404 NotFound
        using var delReqPres = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/files/{storedFileName}");
        delReqPres.Headers.Authorization = new AuthenticationHeaderValue("Bearer", presenterToken);
        var delRespPres = await _client.SendAsync(delReqPres);
        Assert.Equal(HttpStatusCode.NoContent, delRespPres.StatusCode);

        var afterDeleteGet = await _client.GetAsync($"/api/v1/files/{storedFileName}");
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteGet.StatusCode);
    }

    [Fact]
    public async Task Upload_InvalidSpoofedExeAndOversizedFile_Returns400ProblemDetails()
    {
        var token = await LoginAsync("presenter_demo", "Presenter@123456");

        // 1. Virus.exe disguised as .jpg -> 400 BadRequest
        using var spoofForm = CreateSingleFileContent(FakeExeBytes, "virus.jpg", "image/jpeg");
        var spoofResp = await SendMultipartAsync("/api/v1/files/upload", spoofForm, token);
        Assert.Equal(HttpStatusCode.BadRequest, spoofResp.StatusCode);

        // 2. Empty file (0 bytes) -> 400 BadRequest
        using var emptyForm = CreateSingleFileContent(Array.Empty<byte>(), "empty.png", "image/png");
        var emptyResp = await SendMultipartAsync("/api/v1/files/upload", emptyForm, token);
        Assert.Equal(HttpStatusCode.BadRequest, emptyResp.StatusCode);

        // 3. Over 5MB file -> 400 BadRequest
        var hugePayload = new byte[(5 * 1024 * 1024) + 128];
        Array.Copy(ValidPngBytes, hugePayload, ValidPngBytes.Length);
        using var hugeForm = CreateSingleFileContent(hugePayload, "oversized.png", "image/png");
        var hugeResp = await SendMultipartAsync("/api/v1/files/upload", hugeForm, token);
        Assert.Equal(HttpStatusCode.BadRequest, hugeResp.StatusCode);
    }

    [Fact]
    public async Task UploadMultiple_WithOneValidAndOneInvalidFile_Returns200PartialSuccess()
    {
        var token = await LoginAsync("presenter_demo", "Presenter@123456");

        using var form = new MultipartFormDataContent();
        var mdBytes = Encoding.UTF8.GetBytes("# Buổi 10 File Upload\n\nMarkdown hợp lệ.");
        var validPart = new ByteArrayContent(mdBytes);
        validPart.Headers.ContentType = new MediaTypeHeaderValue("text/markdown");
        form.Add(validPart, "files", "guide.md");

        var invalidPart = new ByteArrayContent(FakeExeBytes);
        invalidPart.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(invalidPart, "files", "trojan.png");

        var response = await SendMultipartAsync("/api/v1/files/upload-multiple", form, token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("successCount").GetInt32());
        Assert.Equal(1, data.GetProperty("errorCount").GetInt32());
    }

    [Fact]
    public async Task ConfigEndpoints_ReturnEnvironmentAndEnforceHostRoleWithoutLeakingSecrets()
    {
        // 1. Public environment endpoint
        var envResp = await _client.GetAsync("/api/v1/config/environment");
        Assert.Equal(HttpStatusCode.OK, envResp.StatusCode);
        var envJson = await envResp.Content.ReadAsStringAsync();
        using var envDoc = JsonDocument.Parse(envJson);
        var envData = envDoc.RootElement.GetProperty("data");
        Assert.Equal("Development", envData.GetProperty("environmentName").GetString());
        Assert.True(envData.GetProperty("isDevelopment").GetBoolean());
        Assert.True(envData.GetProperty("features").GetProperty("enableSwagger").GetBoolean());

        // 2. /api/v1/config/options enforces Host role
        var presenterToken = await LoginAsync("presenter_demo", "Presenter@123456");
        using var presReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/config/options");
        presReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", presenterToken);
        var presResp = await _client.SendAsync(presReq);
        Assert.Equal(HttpStatusCode.Forbidden, presResp.StatusCode);

        var hostToken = await LoginAsync("admin_demo", "Admin@123456");
        using var hostReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/config/options");
        hostReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", hostToken);
        var hostResp = await _client.SendAsync(hostReq);
        Assert.Equal(HttpStatusCode.OK, hostResp.StatusCode);

        var hostJson = await hostResp.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SecretKey", hostJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TheEcosystem_Local_Dev", hostJson);
        Assert.Contains("iOptionsSingleton", hostJson);
        Assert.Contains("iOptionsSnapshotScoped", hostJson);
        Assert.Contains("iOptionsMonitorSingleton", hostJson);
    }

    [Fact]
    public async Task ProductionEnvironment_DisablesSwaggerAndAppliesProductionSettings()
    {
        // Arrange: Separate factory simulating Production environment
        await using var prodFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("EcosystemFeatures:EnableAutoMigration", "false");
        });

        using var prodClient = prodFactory.CreateClient();

        // Act
        var envResp = await prodClient.GetAsync("/api/v1/config/environment");
        var swaggerResp = await prodClient.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, envResp.StatusCode);
        using var envDoc = JsonDocument.Parse(await envResp.Content.ReadAsStringAsync());
        var envData = envDoc.RootElement.GetProperty("data");
        Assert.Equal("Production", envData.GetProperty("environmentName").GetString());
        Assert.True(envData.GetProperty("isProduction").GetBoolean());
        Assert.False(envData.GetProperty("features").GetProperty("enableSwagger").GetBoolean());
        Assert.Equal("Production Hardened Mode", envData.GetProperty("features").GetProperty("environmentBanner").GetString());
        Assert.Equal(HttpStatusCode.NotFound, swaggerResp.StatusCode);
    }

    [Fact]
    public async Task OptionsSnapshot_HotReloadsNewConfig_WhileIOptionsStaysSingleton()
    {
        // Arrange: Update override config file and trigger IConfiguration reload inside Host
        var hostToken = await LoginAsync("admin_demo", "Admin@123456");

        File.WriteAllText(_factory.DynamicOverrideConfigPath, """
            {
              "FileStorage": {
                "MaxFileSizeBytes": 2097152
              }
            }
            """);

        // Force host configuration root reload cleanly without timing flakiness
        var rootConfig = _factory.Services.GetService(typeof(IConfiguration)) as IConfigurationRoot;
        Assert.NotNull(rootConfig);
        rootConfig!.Reload();

        // Act: Query /api/v1/config/options as Host
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/config/options");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", hostToken);
        var resp = await _client.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var comparison = doc.RootElement.GetProperty("data").GetProperty("optionsPatternComparison");
        var singletonMax = comparison.GetProperty("iOptionsSingleton").GetProperty("value").GetProperty("maxFileSizeBytes").GetInt64();
        var snapshotMax = comparison.GetProperty("iOptionsSnapshotScoped").GetProperty("value").GetProperty("maxFileSizeBytes").GetInt64();

        // Assert: IOptions stays 5MB from startup, while IOptionsSnapshot immediately picks up 2MB!
        Assert.Equal(5_242_880, singletonMax);
        Assert.Equal(2_097_152, snapshotMax);

        // Restore original 5MB value so sibling tests remain unaffected
        File.WriteAllText(_factory.DynamicOverrideConfigPath, """
            {
              "FileStorage": {
                "MaxFileSizeBytes": 5242880
              }
            }
            """);
        rootConfig.Reload();
    }

    private async Task<string> LoginAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequestDto
        {
            Username = username,
            Password = password
        });
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    private async Task<HttpResponseMessage> SendMultipartAsync(
        string endpoint,
        MultipartFormDataContent content,
        string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }

    private static MultipartFormDataContent CreateSingleFileContent(
        byte[] bytes,
        string fileName,
        string contentType)
    {
        var form = new MultipartFormDataContent();
        var part = new ByteArrayContent(bytes);
        part.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(part, "file", fileName);
        return form;
    }
}
