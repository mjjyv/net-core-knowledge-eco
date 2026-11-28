using Ecosystem.Application.Interfaces;
using Ecosystem.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ecosystem.WebAPI.Services;

/// <summary>
/// BackgroundService thực thi Eviction Policy (30 giây): định kỳ dọn dẹp các kết nối rớt mạng
/// không gửi Heartbeat trong ClassroomHub.
/// </summary>
public class ClassroomHeartbeatMonitorService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(30);

    private readonly IClassroomPresenceTracker _presenceTracker;
    private readonly IHubContext<ClassroomHub> _hubContext;
    private readonly ILogger<ClassroomHeartbeatMonitorService> _logger;

    public ClassroomHeartbeatMonitorService(
        IClassroomPresenceTracker presenceTracker,
        IHubContext<ClassroomHub> hubContext,
        ILogger<ClassroomHeartbeatMonitorService> logger)
    {
        _presenceTracker = presenceTracker;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var evictedList = _presenceTracker.EvictStaleConnections(ConnectionTimeout);
            foreach (var participant in evictedList)
            {
                _logger.LogInformation(
                    "Evicted stale classroom connection {ConnectionId} ({Username}) from room {RoomCode} after 30s timeout.",
                    participant.ConnectionId,
                    participant.Username,
                    participant.RoomCode);

                var groupName = ClassroomHub.GetRoomGroup(participant.RoomCode);
                await _hubContext.Groups.RemoveFromGroupAsync(participant.ConnectionId, groupName, stoppingToken);
                await _hubContext.Clients.Group(groupName).SendAsync("ParticipantEvicted", participant, stoppingToken);
            }
        }
    }
}
