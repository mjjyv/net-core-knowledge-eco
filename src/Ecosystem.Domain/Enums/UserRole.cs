namespace Ecosystem.Domain.Enums;

/// <summary>
/// Vai trò người dùng trong hệ sinh thái The Ecosystem.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Học viên / Người xem (chỉ đọc và tương tác cơ bản)
    /// </summary>
    Audience = 0,

    /// <summary>
    /// Giảng viên / Báo cáo viên (tạo, cập nhật bài viết, điều khiển phiên học)
    /// </summary>
    Presenter = 1,

    /// <summary>
    /// Quản trị viên hệ thống (toàn quyền quản trị)
    /// </summary>
    Host = 2
}
