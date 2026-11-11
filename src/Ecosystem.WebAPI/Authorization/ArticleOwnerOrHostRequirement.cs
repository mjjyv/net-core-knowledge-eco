using Microsoft.AspNetCore.Authorization;

namespace Ecosystem.WebAPI.Authorization;

/// <summary>
/// Requirement kiểm tra quyền sở hữu tài nguyên (tác giả bài viết) hoặc quyền quản trị tối cao Host (Resource-based Policy).
/// </summary>
public class ArticleOwnerOrHostRequirement : IAuthorizationRequirement
{
}
