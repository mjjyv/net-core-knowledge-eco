using Ecosystem.Domain.Common;
using Ecosystem.Domain.Enums;

namespace Ecosystem.Domain.Entities;

/// <summary>
/// Phiên lớp học / cộng tác trực tuyến thời gian thực.
/// </summary>
public class ClassroomSession : BaseEntity, IAggregateRoot
{
    public Guid HostId { get; set; }
    public User Host { get; set; } = null!;

    public string RoomCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}
