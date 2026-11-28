using Ecosystem.Application.DTOs;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Hợp đồng quản lý trạng thái hiện diện và Heartbeat của các kết nối trong ClassroomHub (Eviction Policy 30s).
/// </summary>
public interface IClassroomPresenceTracker
{
    ClassroomParticipantDto JoinRoom(string connectionId, string roomCode, string userId, string username, string role);
    ClassroomParticipantDto? LeaveByConnection(string connectionId);
    bool UpdateHeartbeat(string connectionId);
    ClassroomParticipantDto? GetParticipant(string connectionId);
    IReadOnlyList<ClassroomParticipantDto> GetRoomParticipants(string roomCode);
    IReadOnlyList<ClassroomParticipantDto> EvictStaleConnections(TimeSpan timeout);
}
