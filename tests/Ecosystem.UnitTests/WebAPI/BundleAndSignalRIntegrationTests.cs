using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ecosystem.Application.DTOs;
using Ecosystem.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ecosystem.UnitTests.WebAPI;

public sealed class BundleAndSignalRWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"ecosystem_phase14_test_{Guid.NewGuid():N}.db");

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
                    try { File.Delete(file); } catch { /* ignore SQLite cleanup locks */ }
                }
            }
        }
    }
}

public class BundleAndSignalRIntegrationTests : IClassFixture<BundleAndSignalRWebApplicationFactory>
{
    private static readonly Guid CleanArchCategoryId = Guid.Parse("11111111-aaaa-c111-1111-111111111111");
    private readonly HttpClient _client;

    public BundleAndSignalRIntegrationTests(BundleAndSignalRWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetArticleBundle_ExtractsHierarchyDeepLinkTagsAndMarkdownAttachments()
    {
        // Arrange: Login as Presenter and create an article containing embedded file references
        var token = await LoginAsync("presenter_demo", "Presenter@123456");

        using var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/articles")
        {
            Content = JsonContent.Create(new ArticleCreateDto
            {
                CategoryId = CleanArchCategoryId,
                Title = "Bài Viết Đồng Bộ PKM Bundle Test",
                Slug = $"bundle-pkm-{Guid.NewGuid():N}"[..24],
                Summary = "Kiểm chứng xuất bản ecosystem://bundle",
                MarkdownContent = "# PKM Bundle\n![Diagram](/api/v1/files/arch-diagram.png)\n[Slide](/uploads/slides-14.pdf)",
                Status = ArticleStatus.Published,
                Tags = new List<string> { "pkm", "signalr", "bundle" }
            })
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResp = await _client.SendAsync(createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        using var createDoc = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync());
        var articleId = createDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        // Act: Call GET /api/v1/articles/{id}/bundle anonymously
        var bundleResp = await _client.GetAsync($"/api/v1/articles/{articleId}/bundle");

        // Assert
        Assert.Equal(HttpStatusCode.OK, bundleResp.StatusCode);
        using var bundleDoc = JsonDocument.Parse(await bundleResp.Content.ReadAsStringAsync());
        var data = bundleDoc.RootElement.GetProperty("data");

        Assert.Equal(articleId, data.GetProperty("articleId").GetGuid());
        Assert.Equal("lap-trinh-dotnet-aspnet-core/kien-truc-backend-csharp/clean-architecture-cqrs",
            data.GetProperty("categoryPath").GetString());
        Assert.StartsWith("ecosystem://article/", data.GetProperty("deepLink").GetString());

        var attachments = data.GetProperty("attachments").EnumerateArray()
            .Select(a => a.GetProperty("fileName").GetString())
            .ToList();
        Assert.Contains("arch-diagram.png", attachments);
        Assert.Contains("slides-14.pdf", attachments);

        // Act 2: Non-existent article -> 404 NotFound
        var notFoundResp = await _client.GetAsync($"/api/v1/articles/{Guid.NewGuid()}/bundle");
        Assert.Equal(HttpStatusCode.NotFound, notFoundResp.StatusCode);
    }

    [Theory]
    [InlineData("/hubs/collaboration/negotiate?negotiateVersion=1")]
    [InlineData("/hubs/classroom/negotiate?negotiateVersion=1")]
    public async Task SignalRNegotiate_EnforcesJwtAuthAndAcceptsQueryAccessToken(string negotiatePath)
    {
        // 1. Anonymous negotiate -> 401 Unauthorized
        var anonResp = await _client.PostAsync(negotiatePath, content: null);
        Assert.Equal(HttpStatusCode.Unauthorized, anonResp.StatusCode);

        // 2. Authenticated via SignalR standard ?access_token=... query string -> 200 OK
        var token = await LoginAsync("presenter_demo", "Presenter@123456");
        var authResp = await _client.PostAsync($"{negotiatePath}&access_token={Uri.EscapeDataString(token)}", content: null);

        Assert.Equal(HttpStatusCode.OK, authResp.StatusCode);
        var json = await authResp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("connectionId").GetString()));
        Assert.True(doc.RootElement.GetProperty("availableTransports").GetArrayLength() > 0);
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
}
