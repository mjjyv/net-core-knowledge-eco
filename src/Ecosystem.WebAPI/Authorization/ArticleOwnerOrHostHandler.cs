using System.Security.Claims;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Ecosystem.WebAPI.Authorization;

/// <summary>
/// Resource-based AuthorizationHandler kiểm tra quyền sở hữu bài viết:
/// - Host có toàn quyền chỉnh sửa/xóa mọi bài viết.
/// - Presenter chỉ được chỉnh sửa/xóa bài viết do chính mình tạo (AuthorId == currentUserId).
/// </summary>
public class ArticleOwnerOrHostHandler
    : AuthorizationHandler<ArticleOwnerOrHostRequirement, ArticleDetailResponseDto>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ArticleOwnerOrHostRequirement requirement,
        ArticleDetailResponseDto resource)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(AuthConstants.Roles.Host))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? context.User.FindFirstValue("sub");

        if (Guid.TryParse(userIdClaim, out var currentUserId) && resource.AuthorId == currentUserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
