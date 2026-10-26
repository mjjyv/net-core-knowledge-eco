namespace Ecosystem.Application.Models;

/// <summary>
/// Dữ liệu đầu vào khi tạo mới một Topic.
/// </summary>
public class CreateTopicRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// Dữ liệu đầu vào khi cập nhật một Topic.
/// </summary>
public class UpdateTopicRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// Dữ liệu đầu vào khi tạo mới một Aspect trực thuộc Topic.
/// </summary>
public class CreateAspectRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// Dữ liệu đầu vào khi cập nhật một Aspect.
/// </summary>
public class UpdateAspectRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// Dữ liệu đầu vào khi tạo mới một Category trực thuộc Aspect.
/// </summary>
public class CreateCategoryRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// Dữ liệu đầu vào khi cập nhật một Category.
/// </summary>
public class UpdateCategoryRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// Tham số truy vấn phục vụ lọc, tìm kiếm và sắp xếp danh mục từ Query String.
/// </summary>
public class TaxonomyFilterQuery
{
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "order"; // order, title, createdat
    public bool IsDescending { get; set; } = false;
}
