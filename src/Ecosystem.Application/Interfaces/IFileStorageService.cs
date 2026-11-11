using Ecosystem.Application.DTOs;

namespace Ecosystem.Application.Interfaces;

/// <summary>
/// Hợp đồng dịch vụ lưu trữ, xác thực đa tầng và phân phát tệp đính kèm trong The Ecosystem (Buổi 10).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Lấy thông tin cấu hình giới hạn upload hiện hành từ Options Pattern.
    /// </summary>
    FileLimitsDto GetLimits();

    /// <summary>
    /// Xác thực đa tầng và lưu một tệp lên kho lưu trữ.
    /// </summary>
    Task<FileUploadResultDto> UploadAsync(FileUploadInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xử lý upload nhiều tệp cùng lúc theo mô hình Partial Success (có kiểm tra tổng dung lượng và số tệp).
    /// </summary>
    Task<MultiFileUploadResultDto> UploadMultipleAsync(
        IReadOnlyList<FileUploadInput> inputs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mở luồng đọc tệp đính kèm để tải xuống (chống Path Traversal).
    /// </summary>
    Task<FileDownloadResult?> OpenDownloadAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa tệp đính kèm khỏi kho lưu trữ một cách an toàn.
    /// </summary>
    Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}
