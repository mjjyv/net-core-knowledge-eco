using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Ecosystem.WebAPI.Hubs;

/// <summary>
/// Tiện ích nội bộ trích xuất danh tính người dùng đã xác thực từ SignalR HubCallerContext.
/// </summary>
internal static class HubIdentityHelper
{
    public static (string UserId, string Username, string Role) ExtractIdentity(HubCallerContext context)
    {
        var user = context.User;
        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? user?.FindFirstValue("sub")
                     ?? context.ConnectionId;
        var username = user?.Identity?.Name
                       ?? user?.FindFirstValue(ClaimTypes.Name)
                       ?? "Anonymous";
        var role = user?.FindFirstValue(ClaimTypes.Role) ?? "Audience";

        return (userId, username, role);
    }
}
