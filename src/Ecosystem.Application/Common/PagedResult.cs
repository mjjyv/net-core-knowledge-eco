namespace Ecosystem.Application.Common;

/// <summary>
/// Mô hình phản hồi phân trang chuẩn chứa danh sách dữ liệu của trang hiện tại và metadata điều hướng (Buổi 11).
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu phần tử DTO.</typeparam>
public class PagedResult<T>
{
    public PagedResult()
    {
    }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = Math.Max(0, totalCount);
        PageNumber = Math.Max(1, pageNumber);
        PageSize = Math.Clamp(pageSize, 1, PaginationParams.MaxPageSize);
    }

    /// <summary>
    /// Danh sách phần tử của trang hiện tại.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Số trang hiện tại (bắt đầu từ 1).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Kích thước trang hiện tại (1..50).
    /// </summary>
    public int PageSize { get; set; } = PaginationParams.DefaultPageSize;

    /// <summary>
    /// Tổng số bản ghi khớp điều kiện lọc và tìm kiếm trong CSDL.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Tổng số trang tính toán tự động từ TotalCount và PageSize.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Cờ cho biết có trang trước hay không.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Cờ cho biết có trang tiếp theo hay không.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
