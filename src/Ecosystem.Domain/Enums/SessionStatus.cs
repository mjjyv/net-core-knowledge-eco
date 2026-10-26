namespace Ecosystem.Domain.Enums;

/// <summary>
/// Trạng thái vòng đời của phiên lớp học trực tuyến thời gian thực.
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// Đã lên lịch, chưa bắt đầu
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Đang diễn ra (Active)
    /// </summary>
    Active = 1,

    /// <summary>
    /// Đã kết thúc
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Đã hủy
    /// </summary>
    Cancelled = 3
}
