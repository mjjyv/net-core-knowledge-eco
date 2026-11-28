using System.Net;
using Ecosystem.Application.Common;
using Ecosystem.WebAPI.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecosystem.UnitTests.WebAPI;

public sealed class CorsAndSwaggerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"ecosystem_cors_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("DatabaseProvider", "Sqlite");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
            {
                var file = _dbPath + suffix;
                if (File.Exists(file))
                {
                    try { File.Delete(file); } catch { /* ignore cleanup locks */ }
                }
            }
        }
    }
}

public class CorsAndSwaggerIntegrationTests : IClassFixture<CorsAndSwaggerWebApplicationFactory>
{
    private readonly CorsAndSwaggerWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CorsAndSwaggerIntegrationTests(CorsAndSwaggerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public void CorsOptions_WhenWildcardWithAllowCredentials_ThrowsOptionsValidationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "*",
                ["Cors:AllowCredentials"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddConfigurationAndOptions(config);
        using var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<CorsOptions>>().Value);
        Assert.Contains("AllowedOrigins", ex.Message);
    }

    [Fact]
    public async Task CorsPreflight_OnProtectedEndpointFromAllowedOrigin_Returns204Not401()
    {
        // Arrange: Preflight OPTIONS tới endpoint [Authorize] không mang token -> UseCors phải đứng trước UseAuthentication
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/me");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Authorization, Content-Type");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://localhost:3000", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Equal("true", response.Headers.GetValues("Access-Control-Allow-Credentials").Single());
    }

    [Fact]
    public async Task CorsActualRequest_FromAllowedOrigin_ExposesCustomHeaders()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/articles?pageNumber=1&pageSize=2");
        request.Headers.Add("Origin", "http://localhost:5173");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        var exposed = string.Join(",", response.Headers.GetValues("Access-Control-Expose-Headers"));
        Assert.Contains("X-Pagination", exposed);
        Assert.Contains("X-Request-Id", exposed);
        Assert.Contains("X-Response-Time-Ms", exposed);
    }

    [Fact]
    public async Task CorsRequest_FromDisallowedOrigin_DoesNotReturnAllowOriginHeader()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/articles");
        request.Headers.Add("Origin", "https://evil-attacker.com");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task SwaggerJsonAndUi_InDevelopment_ContainXmlDocsAndBearerSecurity()
    {
        // Act
        var jsonResp = await _client.GetAsync("/swagger/v1/swagger.json");
        var uiResp = await _client.GetAsync("/swagger/index.html");
        var json = await jsonResp.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, jsonResp.StatusCode);
        Assert.Equal(HttpStatusCode.OK, uiResp.StatusCode);
        Assert.Contains("Bearer", json);
        Assert.Contains("X-Pagination", json);
    }

    [Fact]
    public async Task Production_CorsAllowsHttpsOriginAndBlocksLocalhostDevOrigin()
    {
        // Arrange
        await using var prodFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("EcosystemFeatures:EnableAutoMigration", "false");
        });
        using var prodClient = prodFactory.CreateClient();

        using var allowReq = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/me");
        allowReq.Headers.Add("Origin", "https://ecosystem.vn");
        allowReq.Headers.Add("Access-Control-Request-Method", "GET");

        using var blockReq = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/me");
        blockReq.Headers.Add("Origin", "http://localhost:3000");
        blockReq.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var allowResp = await prodClient.SendAsync(allowReq);
        var blockResp = await prodClient.SendAsync(blockReq);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, allowResp.StatusCode);
        Assert.Equal("https://ecosystem.vn", allowResp.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(blockResp.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
