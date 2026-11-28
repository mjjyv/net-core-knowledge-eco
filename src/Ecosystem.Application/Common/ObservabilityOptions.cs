using System.ComponentModel.DataAnnotations;

namespace Ecosystem.Application.Common;

/// <summary>
/// Lớp cấu hình Strongly-Typed cho Giám sát, Structured Logging &amp; Cảnh báo độ trễ (Section "Observability" - Buổi 12).
/// </summary>
public class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>
    /// Ngưỡng thời gian xử lý (ms) để RequestTimingMiddleware phát cảnh báo LogWarning (mặc định: 500ms).
    /// </summary>
    [Range(10, 60_000, ErrorMessage = "SlowRequestThresholdMs phải nằm trong khoảng từ 10 đến 60,000 ms.")]
    public int SlowRequestThresholdMs { get; set; } = 500;

    /// <summary>
    /// Cho phép hiển thị chi tiết ngoại lệ trong phản hồi lỗi 500 hay không (chỉ nên bật ở Development).
    /// </summary>
    public bool IncludeExceptionDetailsInResponse { get; set; }
}
