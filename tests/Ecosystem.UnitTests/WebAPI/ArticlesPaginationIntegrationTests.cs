using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ecosystem.UnitTests.WebAPI;

public sealed class ArticlesPaginationWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"ecosystem_paging_test_{Guid.NewGuid():N}.db");

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

public class ArticlesPaginationIntegrationTests : IClassFixture<ArticlesPaginationWebApplicationFactory>
{
    private static readonly Guid BackendAspectId = Guid.Parse("11111111-aaaa-1111-1111-111111111111");
    private static readonly Guid EfCoreCategoryId = Guid.Parse("11111111-aaaa-c222-1111-111111111111");

    private readonly HttpClient _client;

    public ArticlesPaginationIntegrationTests(ArticlesPaginationWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetArticles_PaginatesAcrossPagesAndSetsXPaginationHeader()
    {
        // Act: Page 1 (pageSize=2)
        var page1Resp = await _client.GetAsync("/api/v1/articles?pageNumber=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, page1Resp.StatusCode);
        Assert.True(page1Resp.Headers.TryGetValues("X-Pagination", out var headerValues));

        using var headerDoc = JsonDocument.Parse(headerValues!.First());
        Assert.Equal(6, headerDoc.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, headerDoc.RootElement.GetProperty("totalPages").GetInt32());
        Assert.True(headerDoc.RootElement.GetProperty("hasNextPage").GetBoolean());
        Assert.False(headerDoc.RootElement.GetProperty("hasPreviousPage").GetBoolean());

        using var page1Doc = JsonDocument.Parse(await page1Resp.Content.ReadAsStringAsync());
        var p1Data = page1Doc.RootElement.GetProperty("data");
        Assert.Equal(2, p1Data.GetProperty("items").GetArrayLength());
        Assert.Equal(6, p1Data.GetProperty("totalCount").GetInt32());

        // Act: Page 3 (Last page)
        var page3Resp = await _client.GetAsync("/api/v1/articles?pageNumber=3&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, page3Resp.StatusCode);

        using var page3Doc = JsonDocument.Parse(await page3Resp.Content.ReadAsStringAsync());
        var p3Data = page3Doc.RootElement.GetProperty("data");
        Assert.Equal(2, p3Data.GetProperty("items").GetArrayLength());
        Assert.True(p3Data.GetProperty("hasPreviousPage").GetBoolean());
        Assert.False(p3Data.GetProperty("hasNextPage").GetBoolean());

        // Act: Out-of-range Page 99 returns empty items while keeping accurate TotalCount
        var page99Resp = await _client.GetAsync("/api/v1/articles?pageNumber=99&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, page99Resp.StatusCode);
        using var page99Doc = JsonDocument.Parse(await page99Resp.Content.ReadAsStringAsync());
        var p99Data = page99Doc.RootElement.GetProperty("data");
        Assert.Equal(0, p99Data.GetProperty("items").GetArrayLength());
        Assert.Equal(6, p99Data.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task GetArticles_ClampsOversizedPageSizeAndHandlesInvalidSortSafely()
    {
        // Act: pageSize=999 -> clamped to 50; malicious sortBy -> safe fallback without SQL error
        var response = await _client.GetAsync(
            "/api/v1/articles?pageNumber=-3&pageSize=999&sortBy=id%3B%20DROP%20TABLE%20Articles--");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("pageNumber").GetInt32());
        Assert.Equal(50, data.GetProperty("pageSize").GetInt32());
        Assert.Equal(6, data.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task GetArticles_FiltersByCategoryAspectStatusAndSearchesTitleOrSummary()
    {
        // 1. Filter by CategoryId + Status=Published (Category2 has 2 Published + 1 Draft)
        var catResp = await _client.GetAsync($"/api/v1/articles?categoryId={EfCoreCategoryId}&status=Published");
        Assert.Equal(HttpStatusCode.OK, catResp.StatusCode);
        using var catDoc = JsonDocument.Parse(await catResp.Content.ReadAsStringAsync());
        Assert.Equal(2, catDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());

        // 2. Filter by AspectId (BackendAspectId has 4 articles: 3 Published + 1 Draft)
        var aspectResp = await _client.GetAsync($"/api/v1/articles?aspectId={BackendAspectId}");
        Assert.Equal(HttpStatusCode.OK, aspectResp.StatusCode);
        using var aspectDoc = JsonDocument.Parse(await aspectResp.Content.ReadAsStringAsync());
        Assert.Equal(4, aspectDoc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());

        // 3. SearchTerm matching Summary ("SignalR")
        var searchResp = await _client.GetAsync("/api/v1/articles?searchTerm=SignalR");
        Assert.Equal(HttpStatusCode.OK, searchResp.StatusCode);
        using var searchDoc = JsonDocument.Parse(await searchResp.Content.ReadAsStringAsync());
        var searchData = searchDoc.RootElement.GetProperty("data");
        Assert.Equal(1, searchData.GetProperty("totalCount").GetInt32());
        Assert.Contains("SignalR", searchData.GetProperty("items")[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetArticles_DynamicSorting_OrdersByViewCountAndTitleCorrectly()
    {
        // 1. Sort by ViewCount DESC
        var descResp = await _client.GetAsync("/api/v1/articles?sortBy=viewCount&isDescending=true&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, descResp.StatusCode);
        using var descDoc = JsonDocument.Parse(await descResp.Content.ReadAsStringAsync());
        var descViews = descDoc.RootElement.GetProperty("data").GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("viewCount").GetInt32())
            .ToList();
        Assert.Equal(descViews.OrderByDescending(v => v).ToList(), descViews);
        Assert.Equal(512, descViews.First());
        Assert.Equal(45, descViews.Last());

        // 2. Sort by Title ASC
        var ascResp = await _client.GetAsync("/api/v1/articles?sortBy=title&isDescending=false&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, ascResp.StatusCode);
        using var ascDoc = JsonDocument.Parse(await ascResp.Content.ReadAsStringAsync());
        var titles = ascDoc.RootElement.GetProperty("data").GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("title").GetString()!)
            .ToList();
        Assert.Equal(titles.OrderBy(t => t, StringComparer.Ordinal).ToList(), titles);
    }
}
