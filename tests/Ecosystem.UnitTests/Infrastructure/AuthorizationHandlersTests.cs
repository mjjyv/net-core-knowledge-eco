using System.Security.Claims;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.WebAPI.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Ecosystem.UnitTests.Infrastructure;

public class AuthorizationHandlersTests
{
    private static readonly DateTime FixedToday = new(2026, 11, 11, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("1998-01-01", true)]
    [InlineData("2008-11-11", true)]   // Đúng ngày sinh nhật lần thứ 18
    [InlineData("2008-11-12", false)]  // Còn 1 ngày nữa mới đủ 18 tuổi
    [InlineData("2010-05-20", false)]  // 16 tuổi
    [InlineData("invalid-date", false)]
    public async Task MinimumAgeHandler_EvaluatesDateOfBirthCorrectly(string dobValue, bool expectedSucceeded)
    {
        // Arrange
        var handler = new MinimumAgeHandler(() => FixedToday);
        var requirement = new MinimumAgeRequirement(18);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "test_user"),
                new Claim(ClaimTypes.DateOfBirth, dobValue)
            },
            "Bearer"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.Equal(expectedSucceeded, context.HasSucceeded);
    }

    [Fact]
    public async Task MinimumAgeHandler_WhenMissingDobClaim_DoesNotSucceed()
    {
        // Arrange
        var handler = new MinimumAgeHandler(() => FixedToday);
        var requirement = new MinimumAgeRequirement(18);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "no_dob_user") },
            "Bearer"));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task ArticleOwnerOrHostHandler_HostCanEditAnyArticle_Succeeds()
    {
        // Arrange
        var hostId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var otherAuthorId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var article = new ArticleDetailResponseDto { Id = Guid.NewGuid(), AuthorId = otherAuthorId };

        var context = CreateArticleAuthContext(hostId, AuthConstants.Roles.Host, authenticated: true, article);
        var handler = new ArticleOwnerOrHostHandler();

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ArticleOwnerOrHostHandler_PresenterEditingOwnArticle_Succeeds()
    {
        // Arrange
        var presenterId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var article = new ArticleDetailResponseDto { Id = Guid.NewGuid(), AuthorId = presenterId };

        var context = CreateArticleAuthContext(presenterId, AuthConstants.Roles.Presenter, authenticated: true, article);
        var handler = new ArticleOwnerOrHostHandler();

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ArticleOwnerOrHostHandler_PresenterEditingOthersArticle_DoesNotSucceed()
    {
        // Arrange
        var presenterId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var adminAuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var article = new ArticleDetailResponseDto { Id = Guid.NewGuid(), AuthorId = adminAuthorId };

        var context = CreateArticleAuthContext(presenterId, AuthConstants.Roles.Presenter, authenticated: true, article);
        var handler = new ArticleOwnerOrHostHandler();

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task ArticleOwnerOrHostHandler_UnauthenticatedUser_DoesNotSucceed()
    {
        // Arrange
        var authorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var article = new ArticleDetailResponseDto { Id = Guid.NewGuid(), AuthorId = authorId };

        var context = CreateArticleAuthContext(authorId, AuthConstants.Roles.Host, authenticated: false, article);
        var handler = new ArticleOwnerOrHostHandler();

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateArticleAuthContext(
        Guid userId,
        string role,
        bool authenticated,
        ArticleDetailResponseDto resource)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = authenticated
            ? new ClaimsIdentity(claims, "Bearer")
            : new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        return new AuthorizationHandlerContext(
            new[] { new ArticleOwnerOrHostRequirement() },
            principal,
            resource);
    }
}
