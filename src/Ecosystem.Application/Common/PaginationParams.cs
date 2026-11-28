namespace Ecosystem.Application.Common;

/// <summary>
/// Lớp tham số truy vấn phân trang, tìm kiếm và sắp xếp dùng chung cho các RESTful GET endpoints (Buổi 11).
/// Tự động chuẩn hóa PageNumber (&gt;= 1) và clamp PageSize trong khoảng [1, 50] để chống DoS.
/// </summary>
public class PaginationParams
{
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 10;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Số trang hiện tại (tối thiểu là 1, mặc định: 1).
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Số bản ghi trên mỗi trang (giá trị trong khoảng [1, 50], mặc định: 10).
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }

    /// <summary>
    /// Từ khóa tìm kiếm chuỗi con trong tiêu đề hoặc tóm tắt.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Tên trường sắp xếp theo whitelist của từng tài nguyên.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Chiều sắp xếp giảm dần (mặc định: true).
    /// </summary>
    public bool IsDescending { get; set; } = true;
}
