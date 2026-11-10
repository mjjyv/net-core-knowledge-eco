using System.ComponentModel.DataAnnotations;
using Ecosystem.Application.Validation;

namespace Ecosystem.Application.DTOs;

/// <summary>
/// DTO minh họa kiểm thử Custom Validation Attribute [MustBeEven] (Bài tập 4 - Buổi 4).
/// </summary>
public class EvenNumberDemoDto
{
    /// <summary>
    /// Tên mô tả đối tượng kiểm thử.
    /// </summary>
    /// <example>Sample Even Check</example>
    [Required(ErrorMessage = "Name là bắt buộc.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name phải từ 2 đến 50 ký tự.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Giá trị số nguyên phải là số chẵn.
    /// </summary>
    /// <example>42</example>
    [Required(ErrorMessage = "Value là bắt buộc.")]
    [MustBeEven(ErrorMessage = "Value phải là số chẵn (chia hết cho 2).")]
    public int Value { get; set; }
}
