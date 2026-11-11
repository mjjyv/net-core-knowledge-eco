using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Mappings;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Entities;
using Ecosystem.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ecosystem.UnitTests.Application;

/// <summary>
/// Kiểm thử độc lập tầng UserService bằng xUnit + Moq (Buổi 7).
/// </summary>
public class UserServiceMoqTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly IMapper _mapper;
    private readonly UserService _service;

    public UserServiceMoqTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepo.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _service = new UserService(_mockUnitOfWork.Object, _mapper, NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_ValidUser_AddsUserAndSavesViaUnitOfWork()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "new_user",
            Email = "new@ecosystem.vn",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Age = 24
        };

        _mockUserRepo.Setup(r => r.ExistsByUsernameAsync("new_user", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockUserRepo.Setup(r => r.ExistsByEmailAsync("new@ecosystem.vn", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert
        Assert.Equal("new_user", result.Username);
        Assert.Equal("new@ecosystem.vn", result.Email);
        Assert.Equal(24, result.Age);
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.ExistsByUsernameAsync("existing_user", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var dto = new UserRegisterDto
        {
            Username = "existing_user",
            Email = "unique@ecosystem.vn",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Age = 25
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAsync(dto));
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsMappedUserDtos()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { new() { Id = Guid.NewGuid(), Username = "user1", Email = "u1@ecosystem.vn" } });

        // Act
        var users = (await _service.GetAllUsersAsync()).ToList();

        // Assert
        Assert.Single(users);
        Assert.Equal("user1", users[0].Username);
    }
}
