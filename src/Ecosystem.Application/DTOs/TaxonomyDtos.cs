using System.ComponentModel.DataAnnotations;
using Ecosystem.Application.Validation;

namespace Ecosystem.Application.DTOs;

/// <summary>
/// DTO gửi lên để tạo mới một Chủ đề (Topic) cấp 1.
/// </summary>
public class TopicCreateDto : IValidatableObject
{
    /// <summary>
    /// Tiêu đề Chủ đề kiến thức (3-100 ký tự).
    /// </summary>
    /// <example>Lập Trình .NET &amp; ASP.NET Core</example>
    [Required(ErrorMessage = "Tiêu đề Chủ đề là bắt buộc.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tiêu đề Chủ đề phải từ 3 đến 100 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn định danh thân thiện chuẩn SEO dạng kebab-case. Nếu để trống sẽ được sinh tự động từ Title.
    /// </summary>
    /// <example>lap-trinh-dotnet-aspnet-core</example>
    [SlugValidation]
    [StringLength(100, ErrorMessage = "Slug không được vượt quá 100 ký tự.")]
    public string? Slug { get; set; }

    /// <summary>
    /// Mô tả chi tiết phạm vi chủ đề.
    /// </summary>
    /// <example>Toàn bộ kiến thức nền tảng và nâng cao về C# và ASP.NET Core 8.</example>
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    /// <summary>
    /// Thứ tự hiển thị ưu tiên (từ 0 đến 10000).
    /// </summary>
    /// <example>1</example>
    [Range(0, 10000, ErrorMessage = "Thứ tự sắp xếp phải nằm trong khoảng từ 0 đến 10000.")]
    public int OrderIndex { get; set; } = 0;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Slug) && Slug.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Slug 'admin' là từ khóa hệ thống dành riêng, không được phép sử dụng.", new[] { nameof(Slug) });
        }
    }
}

/// <summary>
/// DTO gửi lên để cập nhật thông tin Chủ đề (Topic).
/// </summary>
public class TopicUpdateDto : IValidatableObject
{
    /// <summary>
    /// Mã định danh duy nhất của Chủ đề.
    /// </summary>
    /// <example>11111111-1111-1111-1111-111111111111</example>
    [Required(ErrorMessage = "Id của Chủ đề là bắt buộc.")]
    public Guid Id { get; set; }

    /// <summary>
    /// Tiêu đề mới của Chủ đề (3-100 ký tự).
    /// </summary>
    /// <example>Lập Trình .NET 8 &amp; Kiến Trúc Hiện Đại</example>
    [Required(ErrorMessage = "Tiêu đề Chủ đề là bắt buộc.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tiêu đề Chủ đề phải từ 3 đến 100 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Slug chuẩn SEO dạng kebab-case.
    /// </summary>
    /// <example>lap-trinh-dotnet-8-kien-truc-hien-dai</example>
    [SlugValidation]
    [StringLength(100, ErrorMessage = "Slug không được vượt quá 100 ký tự.")]
    public string? Slug { get; set; }

    /// <summary>
    /// Mô tả chi tiết.
    /// </summary>
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    /// <summary>
    /// Thứ tự hiển thị ưu tiên.
    /// </summary>
    [Range(0, 10000, ErrorMessage = "Thứ tự sắp xếp phải nằm trong khoảng từ 0 đến 10000.")]
    public int OrderIndex { get; set; } = 0;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Slug) && Slug.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Slug 'admin' là từ khóa hệ thống dành riêng, không được phép sử dụng.", new[] { nameof(Slug) });
        }
    }
}

/// <summary>
/// DTO trả về thông tin tóm lược của Chủ đề (Topic), bảo mật và không chứa vòng lặp thực thể.
/// </summary>
public class TopicResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int AspectsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO trả về chi tiết Chủ đề cùng danh sách các Khía cạnh trực thuộc (Aspects).
/// </summary>
public class TopicDetailResponseDto : TopicResponseDto
{
    public List<AspectResponseDto> Aspects { get; set; } = new();
}

/// <summary>
/// DTO gửi lên để tạo mới một Khía cạnh (Aspect) cấp 2 trực thuộc Topic.
/// </summary>
public class AspectCreateDto
{
    /// <summary>
    /// Tiêu đề Khía cạnh (3-100 ký tự).
    /// </summary>
    /// <example>Backend Architecture &amp; Design</example>
    [Required(ErrorMessage = "Tiêu đề Khía cạnh là bắt buộc.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tiêu đề Khía cạnh phải từ 3 đến 100 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Slug chuẩn SEO dạng kebab-case.
    /// </summary>
    /// <example>backend-architecture-design</example>
    [SlugValidation]
    [StringLength(100, ErrorMessage = "Slug không được vượt quá 100 ký tự.")]
    public string? Slug { get; set; }

    /// <summary>
    /// Mô tả tóm lược.
    /// </summary>
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    /// <summary>
    /// Thứ tự hiển thị.
    /// </summary>
    [Range(0, 10000, ErrorMessage = "Thứ tự sắp xếp phải nằm trong khoảng từ 0 đến 10000.")]
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// DTO gửi lên để cập nhật một Khía cạnh (Aspect).
/// </summary>
public class AspectUpdateDto
{
    /// <summary>
    /// Mã định danh duy nhất của Khía cạnh.
    /// </summary>
    [Required(ErrorMessage = "Id của Khía cạnh là bắt buộc.")]
    public Guid Id { get; set; }

    /// <summary>
    /// Tiêu đề Khía cạnh (3-100 ký tự).
    /// </summary>
    [Required(ErrorMessage = "Tiêu đề Khía cạnh là bắt buộc.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tiêu đề Khía cạnh phải từ 3 đến 100 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Slug chuẩn SEO dạng kebab-case.
    /// </summary>
    [SlugValidation]
    [StringLength(100, ErrorMessage = "Slug không được vượt quá 100 ký tự.")]
    public string? Slug { get; set; }

    /// <summary>
    /// Mô tả chi tiết.
    /// </summary>
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    /// <summary>
    /// Thứ tự hiển thị.
    /// </summary>
    [Range(0, 10000, ErrorMessage = "Thứ tự sắp xếp phải nằm trong khoảng từ 0 đến 10000.")]
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// DTO trả về thông tin Khía cạnh (Aspect).
/// </summary>
public class AspectResponseDto
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int CategoriesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO trả về chi tiết Khía cạnh cùng danh sách Chuyên mục (Categories) trực thuộc.
/// </summary>
public class AspectDetailResponseDto : AspectResponseDto
{
    public List<CategoryResponseDto> Categories { get; set; } = new();
}

/// <summary>
/// DTO gửi lên để tạo mới một Chuyên mục (Category) cấp 3 trực thuộc Aspect.
/// </summary>
public class CategoryCreateDto
{
    /// <summary>
    /// Tiêu đề Chuyên mục (3-100 ký tự).
    /// </summary>
    /// <example>Clean Architecture &amp; CQRS</example>
    [Required(ErrorMessage = "Tiêu đề Chuyên mục là bắt buộc.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tiêu đề Chuyên mục phải từ 3 đến 100 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Slug chuẩn SEO dạng kebab-case.
    /// </summary>
    /// <example>clean-architecture-cqrs</example>
    [SlugValidation]
    [StringLength(100, ErrorMessage = "Slug không được vượt quá 100 ký tự.")]
    public string? Slug { get; set; }

    /// <summary>
    /// Mô tả chi tiết.
    /// </summary>
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    /// <summary>
    /// Thứ tự hiển thị.
    /// </summary>
    [Range(0, 10000, ErrorMessage = "Thứ tự sắp xếp phải nằm trong khoảng từ 0 đến 10000.")]
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// DTO gửi lên để cập nhật một Chuyên mục (Category).
/// </summary>
public class CategoryUpdateDto
{
    /// <summary>
    /// Mã định danh duy nhất của Chuyên mục.
    /// </summary>
    [Required(ErrorMessage = "Id của Chuyên mục là bắt buộc.")]
    public Guid Id { get; set; }

    /// <summary>
    /// Tiêu đề Chuyên mục (3-100 ký tự).
    /// </summary>
    [Required(ErrorMessage = "Tiêu đề Chuyên mục là bắt buộc.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tiêu đề Chuyên mục phải từ 3 đến 100 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Slug chuẩn SEO dạng kebab-case.
    /// </summary>
    [SlugValidation]
    [StringLength(100, ErrorMessage = "Slug không được vượt quá 100 ký tự.")]
    public string? Slug { get; set; }

    /// <summary>
    /// Mô tả chi tiết.
    /// </summary>
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    /// <summary>
    /// Thứ tự hiển thị.
    /// </summary>
    [Range(0, 10000, ErrorMessage = "Thứ tự sắp xếp phải nằm trong khoảng từ 0 đến 10000.")]
    public int OrderIndex { get; set; } = 0;
}

/// <summary>
/// DTO trả về thông tin Chuyên mục (Category).
/// </summary>
public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public Guid AspectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
