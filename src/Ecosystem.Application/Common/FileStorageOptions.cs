using System.ComponentModel.DataAnnotations;

namespace Ecosystem.Application.Common;

/// <summary>
/// Lớp cấu hình Strongly-Typed cho dịch vụ lưu trữ và xác thực tệp đính kèm (Section "FileStorage" - Buổi 10).
/// Hỗ trợ ValidateDataAnnotations().ValidateOnStart() để phát hiện sai sót cấu hình ngay khi khởi động.
/// </summary>
public class FileStorageOptions : IValidatableObject
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Dung lượng tối đa cho mỗi tệp upload (mặc định 5MB = 5,242,880 bytes).
    /// </summary>
    [Range(1, 52_428_800, ErrorMessage = "MaxFileSizeBytes phải nằm trong khoảng từ 1 byte đến 50MB.")]
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// Tổng dung lượng tối đa cho một HTTP request upload nhiều tệp (mặc định 15MB).
    /// </summary>
    [Range(1, 104_857_600, ErrorMessage = "MaxRequestTotalBytes phải nằm trong khoảng từ 1 byte đến 100MB.")]
    public long MaxRequestTotalBytes { get; set; } = 15 * 1024 * 1024;

    /// <summary>
    /// Số lượng tệp tối đa được phép gửi trong một request upload-multiple.
    /// </summary>
    [Range(1, 50, ErrorMessage = "MaxFilesPerRequest phải nằm trong khoảng từ 1 đến 50 tệp.")]
    public int MaxFilesPerRequest { get; set; } = 5;

    /// <summary>
    /// Thư mục lưu trữ tệp (tương đối theo ContentRootPath hoặc đường dẫn tuyệt đối).
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "UploadFolder không được để trống.")]
    public string UploadFolder { get; set; } = "wwwroot/uploads";

    /// <summary>
    /// Danh sách phần mở rộng tệp (kèm dấu chấm) được phép tải lên.
    /// </summary>
    [Required(ErrorMessage = "AllowedExtensions không được null.")]
    [MinLength(1, ErrorMessage = "Phải cấu hình ít nhất 1 phần mở rộng được phép trong AllowedExtensions.")]
    public string[] AllowedExtensions { get; set; } =
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf", ".md"
    };

    /// <summary>
    /// Danh sách MIME Content-Type được phép tải lên.
    /// </summary>
    [Required(ErrorMessage = "AllowedContentTypes không được null.")]
    [MinLength(1, ErrorMessage = "Phải cấu hình ít nhất 1 MIME Content-Type trong AllowedContentTypes.")]
    public string[] AllowedContentTypes { get; set; } =
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "application/pdf",
        "text/markdown",
        "text/plain"
    };

    /// <summary>
    /// Bật/tắt kiểm tra chữ ký nhị phân thực tế của tệp (Magic Bytes / UTF-8 Text check).
    /// </summary>
    public bool VerifyMagicBytes { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaxRequestTotalBytes < MaxFileSizeBytes)
        {
            yield return new ValidationResult(
                "MaxRequestTotalBytes phải lớn hơn hoặc bằng MaxFileSizeBytes.",
                new[] { nameof(MaxRequestTotalBytes), nameof(MaxFileSizeBytes) });
        }

        if (AllowedExtensions.Any(ext => string.IsNullOrWhiteSpace(ext) || !ext.StartsWith('.')))
        {
            yield return new ValidationResult(
                "Mỗi phần tử trong AllowedExtensions phải bắt đầu bằng dấu chấm (ví dụ: '.png', '.md').",
                new[] { nameof(AllowedExtensions) });
        }
    }
}
