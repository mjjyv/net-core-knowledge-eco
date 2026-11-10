using System.Security.Cryptography;
using System.Text;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ quản lý tài khoản người dùng lưu trữ trong bộ nhớ (In-Memory Lab Buổi 4).
/// </summary>
public class UserService : IUserService
{
    private readonly List<User> _users = new();
    private readonly object _lock = new();

    public UserService()
    {
        // Seed một tài khoản mẫu mặc định
        SeedInitialUsers();
    }

    private void SeedInitialUsers()
    {
        var defaultUser = new User
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Username = "admin_demo",
            Email = "admin@ecosystem.vn",
            PasswordHash = HashPassword("Admin@123456"),
            Role = UserRole.Host,
            CreatedAt = DateTime.UtcNow
        };
        _users.Add(defaultUser);
    }

    public Task<UserResponseDto> RegisterAsync(UserRegisterDto dto)
    {
        lock (_lock)
        {
            // Kiểm tra trùng lặp Username
            if (_users.Any(u => u.Username.Equals(dto.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Tên đăng nhập '{dto.Username}' đã tồn tại trong hệ sinh thái.");
            }

            // Kiểm tra trùng lặp Email
            if (_users.Any(u => u.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Địa chỉ email '{dto.Email}' đã được đăng ký bởi tài khoản khác.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = UserRole.Audience,
                CreatedAt = DateTime.UtcNow
            };

            _users.Add(newUser);

            return Task.FromResult(new UserResponseDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                Age = dto.Age,
                PhoneNumber = dto.PhoneNumber,
                Role = newUser.Role.ToString(),
                CreatedAt = newUser.CreatedAt
            });
        }
    }

    public Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        lock (_lock)
        {
            var dtos = _users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Age = 25, // Giá trị hiển thị mặc định
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            }).ToList();

            return Task.FromResult<IEnumerable<UserResponseDto>>(dtos);
        }
    }

    public Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return Task.FromResult<UserResponseDto?>(null);

            return Task.FromResult<UserResponseDto?>(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Age = 25,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            });
        }
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
