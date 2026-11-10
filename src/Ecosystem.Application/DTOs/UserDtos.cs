using System.ComponentModel.DataAnnotations;
using Ecosystem.Domain.Enums;

namespace Ecosystem.Application.DTOs;

/// <summary>
/// DTO tiếp nhận dữ liệu đăng ký người dùng mới (Bài tập 1 &amp; 3 - Buổi 4).
/// </summary>
public class UserRegisterDto
{
    /// <summary>
    /// Tên đăng nhập của người dùng (3-20 ký tự, chỉ chứa chữ cái, số và dấu gạch dưới).
    /// </summary>
    /// <example>john_doe</example>
    [Required(ErrorMessage = "Username là bắt buộc.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username phải từ 3-20 ký tự.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username chỉ chứa chữ cái, số và dấu gạch dưới.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu đăng nhập (tối thiểu 6 ký tự, gồm ít nhất 1 chữ hoa, 1 chữ thường và 1 chữ số).
    /// </summary>
    /// <example>SecurePass123</example>
    [Required(ErrorMessage = "Password là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Password phải có ít nhất 1 chữ hoa, 1 chữ thường và 1 số.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Xác nhận mật khẩu (phải trùng khớp hoàn toàn với Password).
    /// </summary>
    /// <example>SecurePass123</example>
    [Required(ErrorMessage = "ConfirmPassword là bắt buộc.")]
    [Compare(nameof(Password), ErrorMessage = "Password và ConfirmPassword không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ thư điện tử hợp lệ của người dùng.
    /// </summary>
    /// <example>john.doe@ecosystem.vn</example>
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Tuổi của người dùng (từ 18 đến 60 tuổi).
    /// </summary>
    /// <example>25</example>
    [Required(ErrorMessage = "Age là bắt buộc.")]
    [Range(18, 60, ErrorMessage = "Age phải từ 18 đến 60.")]
    public int Age { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ (tùy chọn).
    /// </summary>
    /// <example>0901234567</example>
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// DTO trả về thông tin người dùng an toàn (không bao gồm Password, Salt hay PasswordHash).
/// </summary>
public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = UserRole.Audience.ToString();
    public DateTime CreatedAt { get; set; }
}
