using Ecosystem.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ecosystem.WebAPI.Hubs;

/// <summary>
/// SignalR Hub phục vụ cộng tác đa người dùng trên tài liệu (/hubs/collaboration - Phase 14).
/// Hỗ trợ đồng bộ con trỏ chuột (Presence) và bong bóng chat ngữ cảnh (Inline Floating Message).
/// </summary>
[Authorize]
public class CollaborationHub : Hub
{
    public async Task JoinDocument(string articleId)
    {
        if (string.IsNullOrWhiteSpace(articleId))
        {
            throw new HubException("Mã tài liệu (articleId) không được để trống.");
        }

        var groupName = GetDocumentGroup(articleId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var (userId, username, _) = HubIdentityHelper.ExtractIdentity(Context);
        await Clients.OthersInGroup(groupName).SendAsync("UserJoinedDocument", new
        {
            connectionId = Context.ConnectionId,
            userId,
            username,
            articleId = articleId.Trim(),
            joinedAtUtc = DateTime.UtcNow
        });
    }

    public async Task LeaveDocument(string articleId)
    {
        if (string.IsNullOrWhiteSpace(articleId))
        {
            return;
        }

        var groupName = GetDocumentGroup(articleId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        var (userId, username, _) = HubIdentityHelper.ExtractIdentity(Context);
        await Clients.OthersInGroup(groupName).SendAsync("UserLeftDocument", new
        {
            connectionId = Context.ConnectionId,
            userId,
            username,
            articleId = articleId.Trim(),
            leftAtUtc = DateTime.UtcNow
        });
    }

    public async Task SendCursorPosition(CursorPositionDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ArticleId))
        {
            throw new HubException("Thiếu mã tài liệu (ArticleId) khi cập nhật vị trí con trỏ.");
        }

        var (userId, username, _) = HubIdentityHelper.ExtractIdentity(Context);
        var payload = new UserCursorDto
        {
            ConnectionId = Context.ConnectionId,
            UserId = userId,
            Username = username,
            ArticleId = dto.ArticleId.Trim(),
            X = dto.X,
            Y = dto.Y,
            BlockPercent = dto.BlockPercent,
            NodeId = dto.NodeId,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await Clients.OthersInGroup(GetDocumentGroup(dto.ArticleId))
            .SendAsync("ReceiveCursorPosition", payload);
    }

    public async Task SendFloatingMessage(FloatingMessageDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ArticleId) || string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new HubException("ArticleId và nội dung tin nhắn nổi (Content) không được để trống.");
        }

        if (dto.Content.Length > 500)
        {
            throw new HubException("Nội dung tin nhắn nổi không được vượt quá 500 ký tự.");
        }

        var (userId, username, _) = HubIdentityHelper.ExtractIdentity(Context);
        var message = new FloatingMessageDto
        {
            MessageId = dto.MessageId == Guid.Empty ? Guid.NewGuid() : dto.MessageId,
            ArticleId = dto.ArticleId.Trim(),
            UserId = userId,
            Username = username,
            Content = dto.Content.Trim(),
            AnchorBlockId = dto.AnchorBlockId,
            OffsetY = dto.OffsetY,
            SentAtUtc = DateTime.UtcNow
        };

        await Clients.Group(GetDocumentGroup(dto.ArticleId))
            .SendAsync("ReceiveFloatingMessage", message);
    }

    private static string GetDocumentGroup(string articleId) => $"doc:{articleId.Trim()}";
}
