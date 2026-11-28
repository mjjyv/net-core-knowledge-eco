using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ecosystem.WebAPI.Hubs;

/// <summary>
/// SignalR Hub điều phối phiên lớp học trực tuyến theo thời gian thực (/hubs/classroom - Phase 14).
/// Phân quyền Host/Presenter cho chuyển slide và hỗ trợ giơ tay + Heartbeat Eviction Policy 30s.
/// </summary>
[Authorize]
public class ClassroomHub : Hub
{
    private readonly IClassroomPresenceTracker _presenceTracker;

    public ClassroomHub(IClassroomPresenceTracker presenceTracker)
    {
        _presenceTracker = presenceTracker;
    }

    public async Task<IReadOnlyList<ClassroomParticipantDto>> JoinClassroom(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode))
        {
            throw new HubException("Mã phòng học (roomCode) không được để trống.");
        }

        var normalizedRoom = roomCode.Trim();
        var (userId, username, role) = HubIdentityHelper.ExtractIdentity(Context);

        var participant = _presenceTracker.JoinRoom(Context.ConnectionId, normalizedRoom, userId, username, role);
        await Groups.AddToGroupAsync(Context.ConnectionId, GetRoomGroup(normalizedRoom));

        await Clients.Group(GetRoomGroup(normalizedRoom)).SendAsync("ParticipantJoined", participant);
        return _presenceTracker.GetRoomParticipants(normalizedRoom);
    }

    public async Task LeaveClassroom(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode))
        {
            return;
        }

        var normalizedRoom = roomCode.Trim();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetRoomGroup(normalizedRoom));

        var left = _presenceTracker.LeaveByConnection(Context.ConnectionId);
        if (left != null)
        {
            await Clients.Group(GetRoomGroup(normalizedRoom)).SendAsync("ParticipantLeft", left);
        }
    }

    public Task Heartbeat(string roomCode)
    {
        _presenceTracker.UpdateHeartbeat(Context.ConnectionId);
        return Task.CompletedTask;
    }

    public async Task BroadcastSlideChange(SlideStateDto state)
    {
        if (state == null || string.IsNullOrWhiteSpace(state.RoomCode))
        {
            throw new HubException("Thiếu mã phòng học (RoomCode) khi đồng bộ slide.");
        }

        var isAllowed = Context.User?.IsInRole(AuthConstants.Roles.Host) == true
                        || Context.User?.IsInRole(AuthConstants.Roles.Presenter) == true;
        if (!isAllowed)
        {
            throw new HubException("Chỉ Host hoặc Presenter mới được phép điều khiển chuyển slide.");
        }

        var (_, username, _) = HubIdentityHelper.ExtractIdentity(Context);
        var normalizedRoom = state.RoomCode.Trim();

        var broadcastState = new SlideStateDto
        {
            RoomCode = normalizedRoom,
            ArticleId = state.ArticleId,
            SlideIndex = state.SlideIndex,
            SectionAnchor = state.SectionAnchor,
            UpdatedBy = username,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await Clients.Group(GetRoomGroup(normalizedRoom)).SendAsync("ReceiveSlideChange", broadcastState);
    }

    public async Task HandRaise(string userId)
    {
        var participant = _presenceTracker.GetParticipant(Context.ConnectionId);
        if (participant == null)
        {
            throw new HubException("Bạn cần tham gia phòng học (JoinClassroom) trước khi giơ tay phát biểu.");
        }

        var (verifiedUserId, username, role) = HubIdentityHelper.ExtractIdentity(Context);
        var effectiveUserId = string.IsNullOrWhiteSpace(verifiedUserId) ? userId : verifiedUserId;

        var notification = new HandRaiseNotificationDto
        {
            RoomCode = participant.RoomCode,
            UserId = effectiveUserId,
            Username = username,
            Role = role,
            RaisedAtUtc = DateTime.UtcNow
        };

        await Clients.Group(GetRoomGroup(participant.RoomCode)).SendAsync("ReceiveHandRaise", notification);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var left = _presenceTracker.LeaveByConnection(Context.ConnectionId);
        if (left != null)
        {
            await Clients.Group(GetRoomGroup(left.RoomCode)).SendAsync("ParticipantLeft", left);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static string GetRoomGroup(string roomCode) => $"room:{roomCode.Trim()}";
}
