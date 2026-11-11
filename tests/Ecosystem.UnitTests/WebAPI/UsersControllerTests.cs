using AutoMapper;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Mappings;
using Ecosystem.Application.Services;
using Ecosystem.Domain.Common;
using Ecosystem.Infrastructure.Data;
using Ecosystem.Infrastructure.Data.Seed;
using Ecosystem.Infrastructure.Repositories;
using Ecosystem.Infrastructure.Security;
using Ecosystem.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecosystem.UnitTests.WebAPI;

public class UsersControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly UnitOfWork _unitOfWork;
    private readonly UserService _userService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        DatabaseSeeder.SeedInitialDataAsync(context).GetAwaiter().GetResult();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();

        _unitOfWork = new UnitOfWork(context);
        _userService = new UserService(_unitOfWork, new BCryptPasswordHasher(), mapper, NullLogger<UserService>.Instance);
        _controller = new UsersController(_userService);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Register_WhenValid_ReturnsCreatedAtActionResult_With201Status()
    {
        // Arrange
        var request = new UserRegisterDto
        {
            Username = "student_phuong",
            Password = "SecurePassword123",
            ConfirmPassword = "SecurePassword123",
            Email = "phuong@university.edu.vn",
            Age = 22,
            PhoneNumber = "0987654321"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(UsersController.GetById), createdResult.ActionName);

        var apiResponse = Assert.IsType<ApiResponse<UserResponseDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("student_phuong", apiResponse.Data?.Username);
        Assert.Equal("phuong@university.edu.vn", apiResponse.Data?.Email);
        Assert.Equal(22, apiResponse.Data?.Age);
    }

    [Fact]
    public async Task Register_WhenDuplicateUsername_ReturnsConflictResult_With409Status()
    {
        // Arrange
        var request1 = new UserRegisterDto
        {
            Username = "unique_developer",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Email = "dev1@ecosystem.vn",
            Age = 25
        };
        await _controller.Register(request1);

        var request2 = new UserRegisterDto
        {
            Username = "unique_developer", // Trùng username
            Password = "Password123",
            ConfirmPassword = "Password123",
            Email = "different@ecosystem.vn",
            Age = 26
        };

        // Act
        var result = await _controller.Register(request2);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Contains("đã tồn tại", problemDetails.Detail);
    }

    [Fact]
    public async Task Register_WhenDuplicateEmail_ReturnsConflictResult_With409Status()
    {
        // Arrange
        var request1 = new UserRegisterDto
        {
            Username = "user_one",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Email = "shared@ecosystem.vn",
            Age = 25
        };
        await _controller.Register(request1);

        var request2 = new UserRegisterDto
        {
            Username = "user_two", // Khác username
            Password = "Password123",
            ConfirmPassword = "Password123",
            Email = "shared@ecosystem.vn", // Trùng email
            Age = 30
        };

        // Act
        var result = await _controller.Register(request2);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Contains("đã được đăng ký", problemDetails.Detail);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithUsersList()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<IEnumerable<UserResponseDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.NotEmpty(apiResponse.Data);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFoundResult_With404Status()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _controller.GetById(nonExistentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Contains("Không tìm thấy người dùng", problemDetails.Title);
    }
}
