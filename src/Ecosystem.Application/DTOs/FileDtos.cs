namespace Ecosystem.Application.DTOs;

/// <summary>
/// Đối tượng đầu vào thuần túy cho tầng Application/Infrastructure — tách biệt hoàn toàn khỏi ASP.NET Core IFormFile.
/// </summary>
public class FileUploadInput
{
    public required Stream Content { get; init; }
    public required string OriginalFileName { get; init; }
    public string ContentType { get; init; } = "application/octet-stream";
    public long Length { get; init; }
}

/// <summary>
/// Kết quả trả về sau khi lưu trữ tệp đính kèm thành công.
/// </summary>
public class FileUploadResultDto
{
    public string StoredFileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string StaticUrl { get; set; } = string.Empty;
    public string Sha256Checksum { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
}

/// <summary>
/// Thông tin lỗi cho từng tệp khi tải lên nhiều tệp đồng thời.
/// </summary>
public class FileUploadErrorDto
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Kết quả tổng hợp cho thao tác tải lên nhiều tệp (Partial Success Pattern).
/// </summary>
public class MultiFileUploadResultDto
{
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<FileUploadResultDto> UploadedFiles { get; set; } = new();
    public List<FileUploadErrorDto> Errors { get; set; } = new();
}

/// <summary>
/// Dữ liệu luồng tệp dùng cho thao tác Download an toàn.
/// </summary>
public sealed class FileDownloadResult : IDisposable, IAsyncDisposable
{
    public required Stream FileStream { get; init; }
    public required string StoredFileName { get; init; }
    public required string ContentType { get; init; }
    public long SizeBytes { get; init; }

    public void Dispose() => FileStream.Dispose();

    public ValueTask DisposeAsync() => FileStream.DisposeAsync();
}

/// <summary>
/// Thông tin cấu hình giới hạn tệp hiện hành đọc qua Options Pattern.
/// </summary>
public class FileLimitsDto
{
    public long MaxFileSizeBytes { get; set; }
    public double MaxFileSizeMegabytes { get; set; }
    public long MaxRequestTotalBytes { get; set; }
    public int MaxFilesPerRequest { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    public bool VerifyMagicBytes { get; set; }
}
