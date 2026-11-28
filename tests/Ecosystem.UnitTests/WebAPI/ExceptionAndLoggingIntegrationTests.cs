using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ecosystem.UnitTests.WebAPI;

public sealed class ObservabilityWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"ecosystem_obs_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        builder.UseSetting("DatabaseProvider", "Sqlite");
        builder.UseSetting("Observability:SlowRequestThresholdMs", "50");
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

public class ExceptionAndLoggingIntegrationTests : IClassFixture<ObservabilityWebApplicationFactory>
{
    private readonly ObservabilityWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExceptionAndLoggingIntegrationTests(ObservabilityWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Development_ErrorDemo_Returns500ProblemDetailsWithDetailsAndCorrelationHeaders()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/diagnostics/error-demo");
        request.Headers.Add("X-Request-Id", "dev-corr-001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("dev-corr-001", response.Headers.GetValues("X-Request-Id").Single());
        Assert.True(response.Headers.Contains("X-Response-Time-Ms"));

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(500, doc.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("dev-corr-001", doc.RootElement.GetProperty("traceId").GetString());
        Assert.Equal("InvalidOperationException", doc.RootElement.GetProperty("exceptionType").GetString());
        Assert.Contains("cố tình phát sinh", doc.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Production_ErrorDemo_HidesInternalExceptionMessage()
    {
        // Arrange
        await using var prodFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("EcosystemFeatures:EnableAutoMigration", "false");
        });
        using var prodClient = prodFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/diagnostics/error-demo");
        request.Headers.Add("X-Request-Id", "prod-corr-777");

        // Act
        var response = await prodClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("prod-corr-777", response.Headers.GetValues("X-Request-Id").Single());

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var detail = doc.RootElement.GetProperty("detail").GetString()!;
        Assert.DoesNotContain("cố tình phát sinh", detail);
        Assert.False(doc.RootElement.TryGetProperty("exceptionType", out _));
        Assert.Equal("prod-corr-777", doc.RootElement.GetProperty("requestId").GetString());
    }

    [Theory]
    [InlineData("notfound", HttpStatusCode.NotFound, 404)]
    [InlineData("argument", HttpStatusCode.BadRequest, 400)]
    [InlineData("unauthorized", HttpStatusCode.Unauthorized, 401)]
    public async Task ErrorDemo_MapsKnownExceptionsToExpectedProblemDetailsStatus(
        string type,
        HttpStatusCode expectedCode,
        int expectedStatus)
    {
        var response = await _client.GetAsync($"/api/v1/diagnostics/error-demo?type={type}");

        Assert.Equal(expectedCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(expectedStatus, doc.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task SlowDemo_EchoesRequestIdAndMeasuresResponseTimeHeader()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/diagnostics/slow-demo?delayMs=100");
        request.Headers.Add("X-Request-Id", "slow-req-123");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("slow-req-123", response.Headers.GetValues("X-Request-Id").Single());
        var elapsedHeader = response.Headers.GetValues("X-Response-Time-Ms").Single();
        Assert.True(long.TryParse(elapsedHeader, out var elapsedMs) && elapsedMs >= 80);
    }
}
