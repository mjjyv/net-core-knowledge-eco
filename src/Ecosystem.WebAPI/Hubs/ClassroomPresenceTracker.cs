using System.Collections.Concurrent;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;

namespace Ecosystem.WebAPI.Hubs;

/// <summary>
/// Quản lý danh sách kết nối hiện diện trong ClassroomHub bằng ConcurrentDictionary an toàn đa luồng,
/// hỗ trợ Eviction Policy 30 giây cho kết nối rớt mạng.
/// </summary>
public class ClassroomPresenceTracker : IClassroomPresenceTracker
{
    private readonly ConcurrentDictionary<string, ClassroomParticipantDto> _connections = new();
    private readonly Func<DateTime> _utcNowProvider;

    public ClassroomPresenceTracker() : this(() => DateTime.UtcNow)
    {
    }

    public ClassroomPresenceTracker(Func<DateTime> utcNowProvider)
    {
        _utcNowProvider = utcNowProvider;
    }

    public ClassroomParticipantDto JoinRoom(
        string connectionId,
        string roomCode,
        string userId,
        string username,
        string role)
    {
        var now = _utcNowProvider();
        var participant = new ClassroomParticipantDto
        {
            ConnectionId = connectionId,
            RoomCode = roomCode.Trim(),
            UserId = userId,
            Username = username,
            Role = role,
            JoinedAtUtc = now,
            LastHeartbeatUtc = now
        };

        _connections[connectionId] = participant;
        return participant;
    }

    public ClassroomParticipantDto? LeaveByConnection(string connectionId)
    {
        return _connections.TryRemove(connectionId, out var removed) ? removed : null;
    }

    public bool UpdateHeartbeat(string connectionId)
    {
        if (!_connections.TryGetValue(connectionId, out var existing))
        {
            return false;
        }

        existing.LastHeartbeatUtc = _utcNowProvider();
        return true;
    }

    public ClassroomParticipantDto? GetParticipant(string connectionId)
    {
        return _connections.TryGetValue(connectionId, out var participant) ? participant : null;
    }

    public IReadOnlyList<ClassroomParticipantDto> GetRoomParticipants(string roomCode)
    {
        var normalized = roomCode.Trim();
        return _connections.Values
            .Where(p => p.RoomCode.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.JoinedAtUtc)
            .ToList();
    }

    public IReadOnlyList<ClassroomParticipantDto> EvictStaleConnections(TimeSpan timeout)
    {
        var cutoff = _utcNowProvider().Subtract(timeout);
        var evicted = new List<ClassroomParticipantDto>();

        foreach (var pair in _connections)
        {
            if (pair.Value.LastHeartbeatUtc < cutoff && _connections.TryRemove(pair.Key, out var removed))
            {
                evicted.Add(removed);
            }
        }

        return evicted;
    }
}
