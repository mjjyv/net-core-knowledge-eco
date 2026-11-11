using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ecosystem.UnitTests.WebAPI;

public sealed class AuthorizationWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"ecosystem_authz_test_{Guid.NewGuid():N}.db");

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

public class AuthorizationMatrixIntegrationTests : IClassFixture<AuthorizationWebApplicationFactory>
{
    private static readonly Guid HostUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PresenterUserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid HostArticleId = Guid.Parse("11111111-aaaa-c111-a001-111111111111");
    private static readonly Guid PresenterArticleId = Guid.Parse("22222222-aaaa-c111-a002-222222222222");
    private static readonly Guid CleanArchCategoryId = Guid.Parse("11111111-aaaa-c111-1111-111111111111");
    private static readonly Guid EfCoreCategoryId = Guid.Parse("11111111-aaaa-c222-1111-111111111111");

    private readonly AuthorizationWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthorizationMatrixIntegrationTests(AuthorizationWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/api/v1/topics")]
    [InlineData("/api/v1/articles")]
    [InlineData("/api/v1/taxonomy/overview")]
    public async Task Anonymous_PublicEndpoints_Return200Ok(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/v1/auth/me")]
    [InlineData("GET", "/api/v1/auth/claims")]
    [InlineData("GET", "/api/v1/auth/adult-verified")]
    [InlineData("GET", "/api/v1/students")]
    [InlineData("GET", "/api/v1/users")]
    [InlineData("POST", "/api/v1/topics")]
    [InlineData("POST", "/api/v1/articles")]
    [InlineData("PUT", "/api/v1/articles/11111111-aaaa-c111-a001-111111111111")]
    [InlineData("DELETE", "/api/v1/articles/11111111-aaaa-c111-a001-111111111111")]
    public async Task Anonymous_ProtectedEndpoints_Return401WithBearerChallengeAndProblemDetails(string method, string endpoint)
    {
        // Act
        var response = await SendAsync(new HttpMethod(method), endpoint, token: null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains(response.Headers.WwwAuthenticate, h => h.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase));
        await AssertProblemDetailsAsync(response, 401);
    }

    [Fact]
    public async Task AudienceRole_EnforcesReadStudentAllowedAndWriteOrAdminForbidden403()
    {
        // Arrange
        var token = await LoginAsync("user_demo", "User@123456");

        // Act & Assert 1: Authenticated user can read /students
        var studentsGet = await SendAsync(HttpMethod.Get, "/api/v1/students", token);
        Assert.Equal(HttpStatusCode.OK, studentsGet.StatusCode);

        // Act & Assert 2: Audience cannot access Host-only /users
        var usersGet = await SendAsync(HttpMethod.Get, "/api/v1/users", token);
        Assert.Equal(HttpStatusCode.Forbidden, usersGet.StatusCode);
        await AssertProblemDetailsAsync(usersGet, 403);

        // Act & Assert 3: Audience cannot create Topics (Host only)
        var createTopic = await SendAsync(HttpMethod.Post, "/api/v1/topics", token, new TopicCreateDto
        {
            Title = "Audience Forbidden Topic",
            Slug = "audience-forbidden-topic",
            OrderIndex = 99
        });
        Assert.Equal(HttpStatusCode.Forbidden, createTopic.StatusCode);
        await AssertProblemDetailsAsync(createTopic, 403);

        // Act & Assert 4: Audience cannot create Articles or Students (RequireContentCreator)
        var createArticle = await SendAsync(HttpMethod.Post, "/api/v1/articles", token, new ArticleCreateDto
        {
            CategoryId = CleanArchCategoryId,
            Title = "Audience Should Not Create Article",
            MarkdownContent = "# Forbidden",
            Status = ArticleStatus.Published
        });
        Assert.Equal(HttpStatusCode.Forbidden, createArticle.StatusCode);

        var createStudent = await SendAsync(HttpMethod.Post, "/api/v1/students", token, new StudentCreateDto
        {
            Name = "Sinh Viên Test",
            Major = "CNTT",
            GPA = 3.5
        });
        Assert.Equal(HttpStatusCode.Forbidden, createStudent.StatusCode);
    }

    [Fact]
    public async Task PresenterRole_EnforcesTopicRestrictionAndArticleResourceOwnership()
    {
        // Arrange
        var presenterToken = await LoginAsync("presenter_demo", "Presenter@123456");

        // 1. Presenter cannot create Topic (RequireHost) -> 403
        var createTopic = await SendAsync(HttpMethod.Post, "/api/v1/topics", presenterToken, new TopicCreateDto
        {
            Title = "Presenter Forbidden Topic",
            Slug = "presenter-forbidden-topic",
            OrderIndex = 10
        });
        Assert.Equal(HttpStatusCode.Forbidden, createTopic.StatusCode);

        // 2. Presenter cannot read Users list (Roles = Host) -> 403
        var usersGet = await SendAsync(HttpMethod.Get, "/api/v1/users", presenterToken);
        Assert.Equal(HttpStatusCode.Forbidden, usersGet.StatusCode);

        // 3. Presenter CANNOT edit or delete Host's Article -> 403 Forbidden (ArticleOwnerOrHost)
        var updateHostArticle = await SendAsync(HttpMethod.Put, $"/api/v1/articles/{HostArticleId}", presenterToken, new ArticleUpdateDto
        {
            CategoryId = CleanArchCategoryId,
            Title = "Attempt To Hijack Host Article",
            MarkdownContent = "# Hijack",
            Status = ArticleStatus.Published
        });
        Assert.Equal(HttpStatusCode.Forbidden, updateHostArticle.StatusCode);
        await AssertProblemDetailsAsync(updateHostArticle, 403);

        var deleteHostArticle = await SendAsync(HttpMethod.Delete, $"/api/v1/articles/{HostArticleId}", presenterToken);
        Assert.Equal(HttpStatusCode.Forbidden, deleteHostArticle.StatusCode);

        // 4. Presenter CAN create an Article (and AuthorId spoofing toward Host is overridden to PresenterId)
        var uniqueSlug = $"presenter-owned-article-{Guid.NewGuid():N}"[..36];
        var createOwnArticle = await SendAsync(HttpMethod.Post, "/api/v1/articles", presenterToken, new ArticleCreateDto
        {
            CategoryId = EfCoreCategoryId,
            AuthorId = HostUserId, // cố tình giả mạo HostUserId
            Title = "Bài Viết Của Giảng Viên Presenter",
            Slug = uniqueSlug,
            Summary = "Tóm tắt bài viết hợp lệ",
            MarkdownContent = "# Nội dung bài viết do Presenter tạo",
            Status = ArticleStatus.Published
        });
        Assert.Equal(HttpStatusCode.Created, createOwnArticle.StatusCode);

        using var createdDoc = JsonDocument.Parse(await createOwnArticle.Content.ReadAsStringAsync());
        var createdData = createdDoc.RootElement.GetProperty("data");
        var createdArticleId = createdData.GetProperty("id").GetGuid();
        Assert.Equal(PresenterUserId, createdData.GetProperty("authorId").GetGuid());

        // 5. Presenter CAN edit their own seeded article
        var updateOwnArticle = await SendAsync(HttpMethod.Put, $"/api/v1/articles/{PresenterArticleId}", presenterToken, new ArticleUpdateDto
        {
            CategoryId = EfCoreCategoryId,
            Title = "Hướng Dẫn EF Core LINQ Và N-Tier Architecture Trong .NET 8 (Đã Cập Nhật)",
            Slug = "huong-dan-ef-core-linq-va-n-tier-architecture",
            Summary = "Tóm tắt đã cập nhật bởi chính chủ Presenter.",
            MarkdownContent = "# Cập nhật bởi Presenter",
            Status = ArticleStatus.Published
        });
        Assert.Equal(HttpStatusCode.OK, updateOwnArticle.StatusCode);

        // 6. Presenter CAN create Student
        var createStudent = await SendAsync(HttpMethod.Post, "/api/v1/students", presenterToken, new StudentCreateDto
        {
            Name = "Nguyễn Văn Học Viên",
            Major = "Kỹ Thuật Phần Mềm",
            GPA = 3.85
        });
        Assert.Equal(HttpStatusCode.Created, createStudent.StatusCode);

        // Cleanup presenter's extra article by Presenter self-delete -> 204 NoContent
        var deleteOwnExtra = await SendAsync(HttpMethod.Delete, $"/api/v1/articles/{createdArticleId}", presenterToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteOwnExtra.StatusCode);
    }

    [Fact]
    public async Task HostRole_CanManageUsersTopicsAndOverrideArticleOwnership()
    {
        // Arrange
        var hostToken = await LoginAsync("admin_demo", "Admin@123456");
        var presenterToken = await LoginAsync("presenter_demo", "Presenter@123456");

        // 1. Host CAN list Users -> 200 OK
        var usersGet = await SendAsync(HttpMethod.Get, "/api/v1/users", hostToken);
        Assert.Equal(HttpStatusCode.OK, usersGet.StatusCode);

        // 2. Host CAN create & delete Topics -> 201 Created & 204 NoContent
        var topicSlug = $"host-topic-{Guid.NewGuid():N}"[..24];
        var createTopic = await SendAsync(HttpMethod.Post, "/api/v1/topics", hostToken, new TopicCreateDto
        {
            Title = "Host Managed Topic",
            Slug = topicSlug,
            OrderIndex = 50
        });
        Assert.Equal(HttpStatusCode.Created, createTopic.StatusCode);

        using var topicDoc = JsonDocument.Parse(await createTopic.Content.ReadAsStringAsync());
        var topicId = topicDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var deleteTopic = await SendAsync(HttpMethod.Delete, $"/api/v1/topics/{topicId}", hostToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteTopic.StatusCode);

        // 3. Presenter creates a temporary article, then Host edits & deletes it -> 200 OK & 204 NoContent
        var articleSlug = $"host-override-{Guid.NewGuid():N}"[..28];
        var presenterCreate = await SendAsync(HttpMethod.Post, "/api/v1/articles", presenterToken, new ArticleCreateDto
        {
            CategoryId = EfCoreCategoryId,
            Title = "Bài Viết Tạm Để Host Kiểm Duyệt",
            Slug = articleSlug,
            MarkdownContent = "# Moderation Test",
            Status = ArticleStatus.Draft
        });
        Assert.Equal(HttpStatusCode.Created, presenterCreate.StatusCode);

        using var articleDoc = JsonDocument.Parse(await presenterCreate.Content.ReadAsStringAsync());
        var tempArticleId = articleDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var hostEdit = await SendAsync(HttpMethod.Put, $"/api/v1/articles/{tempArticleId}", hostToken, new ArticleUpdateDto
        {
            CategoryId = EfCoreCategoryId,
            Title = "Host Đã Kiểm Duyệt Bài Viết Này",
            Slug = articleSlug,
            MarkdownContent = "# Moderated By Host",
            Status = ArticleStatus.Published
        });
        Assert.Equal(HttpStatusCode.OK, hostEdit.StatusCode);

        var hostDelete = await SendAsync(HttpMethod.Delete, $"/api/v1/articles/{tempArticleId}", hostToken);
        Assert.Equal(HttpStatusCode.NoContent, hostDelete.StatusCode);
    }

    [Fact]
    public async Task AuthClaimsAndAdultVerified_ReturnExpectedClaimsAndEnforceAtLeast18Policy()
    {
        // Arrange
        var presenterToken = await LoginAsync("presenter_demo", "Presenter@123456");

        // Act 1: /api/v1/auth/claims
        var claimsResp = await SendAsync(HttpMethod.Get, "/api/v1/auth/claims", presenterToken);
        Assert.Equal(HttpStatusCode.OK, claimsResp.StatusCode);

        using var claimsDoc = JsonDocument.Parse(await claimsResp.Content.ReadAsStringAsync());
        var claimsData = claimsDoc.RootElement.GetProperty("data");
        Assert.Equal("presenter_demo", claimsData.GetProperty("username").GetString());
        Assert.Equal(AuthConstants.Departments.Education, claimsData.GetProperty("department").GetString());
        Assert.Equal("1998-01-01", claimsData.GetProperty("dateOfBirth").GetString());
        Assert.Contains(AuthConstants.Roles.Presenter, claimsData.GetProperty("roles").EnumerateArray().Select(r => r.GetString()));

        // Act 2: /api/v1/auth/adult-verified with valid adult user (>= 18) -> 200 OK
        var adultOk = await SendAsync(HttpMethod.Get, "/api/v1/auth/adult-verified", presenterToken);
        Assert.Equal(HttpStatusCode.OK, adultOk.StatusCode);

        // Act 3: /api/v1/auth/adult-verified with under-18 DOB claim -> 403 Forbidden
        var underageToken = CreateCustomDobToken("2015-01-01");
        var adultForbidden = await SendAsync(HttpMethod.Get, "/api/v1/auth/adult-verified", underageToken);
        Assert.Equal(HttpStatusCode.Forbidden, adultForbidden.StatusCode);
        await AssertProblemDetailsAsync(adultForbidden, 403);
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
        var token = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        return token!;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string requestUri,
        string? token,
        object? payload = null)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (payload != null)
        {
            request.Content = JsonContent.Create(payload);
        }

        return await _client.SendAsync(request);
    }

    private static async Task AssertProblemDetailsAsync(HttpResponseMessage response, int expectedStatus)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(expectedStatus, doc.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("title").GetString()));
    }

    private string CreateCustomDobToken(string dobValue)
    {
        var jwtSettings = _factory.Services.GetRequiredService<IOptions<JwtSettings>>().Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, PresenterUserId.ToString()),
            new Claim(ClaimTypes.Name, "minor_demo"),
            new Claim(ClaimTypes.Role, AuthConstants.Roles.Audience),
            new Claim(ClaimTypes.DateOfBirth, dobValue)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
