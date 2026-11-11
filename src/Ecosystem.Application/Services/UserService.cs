using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Enums;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecosystem.Application.Services;

/// <summary>
/// Dịch vụ quản lý tài khoản người dùng sử dụng IUnitOfWork, IPasswordHasher (BCrypt) và AutoMapper.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        ILogger<UserService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger ?? NullLogger<UserService>.Instance;
    }

    public async Task<UserResponseDto> RegisterAsync(UserRegisterDto dto)
    {
        _logger.LogInformation("Yêu cầu đăng ký tài khoản mới cho Username '{Username}'", dto.Username);

        if (await _unitOfWork.Users.ExistsByUsernameAsync(dto.Username))
        {
            throw new InvalidOperationException($"Tên đăng nhập '{dto.Username}' đã tồn tại trong hệ sinh thái.");
        }

        if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
        {
            throw new InvalidOperationException($"Địa chỉ email '{dto.Email}' đã được đăng ký bởi tài khoản khác.");
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username.Trim(),
            Email = dto.Email.Trim(),
            FullName = dto.Username.Trim(),
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Role = UserRole.Audience,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Đã đăng ký thành công tài khoản {UserId} ({Username})", newUser.Id, newUser.Username);

        var response = _mapper.Map<UserResponseDto>(newUser);
        response.Age = dto.Age;
        response.PhoneNumber = dto.PhoneNumber;
        return response;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return _mapper.Map<IEnumerable<UserResponseDto>>(users);
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        return user == null ? null : _mapper.Map<UserResponseDto>(user);
    }
}
