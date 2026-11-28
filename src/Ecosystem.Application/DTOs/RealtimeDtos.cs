using System.ComponentModel.DataAnnotations;

namespace Ecosystem.Application.DTOs;

/// <summary>
/// Tọa độ con trỏ thời gian thực do client gửi lên CollaborationHub.
/// </summary>
public class CursorPositionDto
{
    [Required]
    public string ArticleId { get; set; } = string.Empty;

    public double X { get; set; }
    public double Y { get; set; }
    public double BlockPercent { get; set; }
    public string? NodeId { get; set; }
}

/// <summary>
/// Thông tin con trỏ kèm danh tính người dùng phát tán tới các client khác trong tài liệu.
/// </summary>
public class UserCursorDto
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ArticleId { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double BlockPercent { get; set; }
    public string? NodeId { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tin nhắn nổi ngữ cảnh gắn theo khối nội dung trên tài liệu cộng tác.
/// </summary>
public class FloatingMessageDto
{
    public Guid MessageId { get; set; } = Guid.NewGuid();

    [Required]
    public string ArticleId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;

    public string? AnchorBlockId { get; set; }
    public double OffsetY { get; set; }
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Trạng thái đồng bộ slide/phân đoạn bài giảng trong ClassroomHub.
/// </summary>
public class SlideStateDto
{
    [Required]
    public string RoomCode { get; set; } = string.Empty;

    public Guid? ArticleId { get; set; }
    public int SlideIndex { get; set; }
    public string? SectionAnchor { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Sự kiện giơ tay phát biểu trong phòng học trực tuyến.
/// </summary>
public class HandRaiseNotificationDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime RaisedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Thông tin thành viên đang hiện diện trong một phòng học ClassroomHub.
/// </summary>
public class ClassroomParticipantDto
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public DateTime JoinedAtUtc { get; set; }
    public DateTime LastHeartbeatUtc { get; set; }
}
