using Ecosystem.Application.DTOs;

namespace Ecosystem.Application.Models;

/// <summary>
/// Dữ liệu đầu vào khi tạo mới một Topic (Kế thừa TopicCreateDto có đầy đủ Data Annotations).
/// </summary>
public class CreateTopicRequest : TopicCreateDto
{
}

/// <summary>
/// Dữ liệu đầu vào khi cập nhật một Topic (Kế thừa TopicUpdateDto có đầy đủ Data Annotations).
/// </summary>
public class UpdateTopicRequest : TopicUpdateDto
{
}

/// <summary>
/// Dữ liệu đầu vào khi tạo mới một Aspect (Kế thừa AspectCreateDto có đầy đủ Data Annotations).
/// </summary>
public class CreateAspectRequest : AspectCreateDto
{
}

/// <summary>
/// Dữ liệu đầu vào khi cập nhật một Aspect (Kế thừa AspectUpdateDto có đầy đủ Data Annotations).
/// </summary>
public class UpdateAspectRequest : AspectUpdateDto
{
}

/// <summary>
/// Dữ liệu đầu vào khi tạo mới một Category (Kế thừa CategoryCreateDto có đầy đủ Data Annotations).
/// </summary>
public class CreateCategoryRequest : CategoryCreateDto
{
}

/// <summary>
/// Dữ liệu đầu vào khi cập nhật một Category (Kế thừa CategoryUpdateDto có đầy đủ Data Annotations).
/// </summary>
public class UpdateCategoryRequest : CategoryUpdateDto
{
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
