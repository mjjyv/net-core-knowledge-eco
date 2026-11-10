using System.ComponentModel.DataAnnotations;

namespace Ecosystem.Application.DTOs;

/// <summary>
/// DTO tiếp nhận yêu cầu thêm mới Sinh viên (Lab thực hành Buổi 6 - EF Core CRUD).
/// </summary>
public class StudentCreateDto
{
    /// <summary>
    /// Họ và tên sinh viên.
    /// </summary>
    /// <example>Lê Minh Hoàng</example>
    [Required(ErrorMessage = "Họ tên sinh viên không được để trống.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên sinh viên phải từ 2 đến 150 ký tự.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Chuyên ngành theo học.
    /// </summary>
    /// <example>Kỹ thuật phần mềm</example>
    [Required(ErrorMessage = "Chuyên ngành không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên chuyên ngành không được vượt quá 100 ký tự.")]
    public string Major { get; set; } = string.Empty;

    /// <summary>
    /// Điểm trung bình tích lũy (GPA thang 4.0).
    /// </summary>
    /// <example>3.75</example>
    [Range(0.0, 4.0, ErrorMessage = "Điểm GPA phải nằm trong khoảng từ 0.0 đến 4.0.")]
    public double GPA { get; set; }
}

/// <summary>
/// DTO tiếp nhận yêu cầu cập nhật thông tin Sinh viên.
/// </summary>
public class StudentUpdateDto
{
    /// <summary>
    /// Họ và tên sinh viên.
    /// </summary>
    [Required(ErrorMessage = "Họ tên sinh viên không được để trống.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên sinh viên phải từ 2 đến 150 ký tự.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Chuyên ngành theo học.
    /// </summary>
    [Required(ErrorMessage = "Chuyên ngành không được để trống.")]
    [StringLength(100, ErrorMessage = "Tên chuyên ngành không được vượt quá 100 ký tự.")]
    public string Major { get; set; } = string.Empty;

    /// <summary>
    /// Điểm trung bình tích lũy (GPA thang 4.0).
    /// </summary>
    [Range(0.0, 4.0, ErrorMessage = "Điểm GPA phải nằm trong khoảng từ 0.0 đến 4.0.")]
    public double GPA { get; set; }
}

/// <summary>
/// DTO phản hồi thông tin Sinh viên.
/// </summary>
public class StudentResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public double GPA { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
