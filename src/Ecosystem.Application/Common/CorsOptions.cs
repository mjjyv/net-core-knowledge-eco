using System.ComponentModel.DataAnnotations;

namespace Ecosystem.Application.Common;

/// <summary>
/// Lớp cấu hình Strongly-Typed cho chính sách CORS bảo mật theo môi trường (Section "Cors" - Buổi 13).
/// </summary>
public class CorsOptions : IValidatableObject
{
    public const string SectionName = "Cors";
    public const string PolicyName = "EcosystemFrontendPolicy";

    /// <summary>
    /// Danh sách Origin (domain Frontend) được phép truy cập API chéo nguồn.
    /// </summary>
    [Required(ErrorMessage = "AllowedOrigins không được null.")]
    [MinLength(1, ErrorMessage = "Phải cấu hình ít nhất 1 Origin trong AllowedOrigins.")]
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Cho phép gửi thông tin xác thực (Authorization / Cookies / SignalR) qua CORS.
    /// </summary>
    public bool AllowCredentials { get; set; } = true;

    /// <summary>
    /// Danh sách Custom Response Headers mà JavaScript phía Client được phép đọc.
    /// </summary>
    public string[] ExposedHeaders { get; set; } =
    {
        "X-Pagination",
        "X-Request-Id",
        "X-Response-Time-Ms"
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AllowCredentials && AllowedOrigins.Any(o => o.Trim() == "*"))
        {
            yield return new ValidationResult(
                "Không được phép dùng wildcard '*' trong AllowedOrigins khi AllowCredentials = true.",
                new[] { nameof(AllowedOrigins), nameof(AllowCredentials) });
        }
    }
}
