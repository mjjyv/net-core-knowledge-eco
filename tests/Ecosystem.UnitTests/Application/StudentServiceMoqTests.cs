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
/// Kiểm thử độc lập tầng StudentService bằng xUnit + Moq không phụ thuộc CSDL thật (Bài tập 1 - Buổi 7).
/// </summary>
public class StudentServiceMoqTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IStudentRepository> _mockStudentRepo;
    private readonly IMapper _mapper;
    private readonly StudentService _service;

    public StudentServiceMoqTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockStudentRepo = new Mock<IStudentRepository>();
        _mockUnitOfWork.Setup(u => u.Students).Returns(_mockStudentRepo.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _service = new StudentService(_mockUnitOfWork.Object, _mapper, NullLogger<StudentService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedStudentDtos()
    {
        // Arrange
        _mockStudentRepo.Setup(r => r.SearchByNameAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Student>
            {
                new("Nguyễn Văn An", "CNTT", 3.8) { Id = Guid.NewGuid() },
                new("Trần Thị Bình", "KHMT", 3.9) { Id = Guid.NewGuid() }
            });

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Name == "Nguyễn Văn An");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _mockStudentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidStudent_AddsAndCommitsViaUnitOfWork()
    {
        // Arrange
        var dto = new StudentCreateDto { Name = "Lê Hoàng", Major = "SE", GPA = 3.75 };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.Equal("Lê Hoàng", result.Name);
        Assert.Equal(3.75, result.GPA);
        _mockStudentRepo.Verify(r => r.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenStudentNotFound_ReturnsFalse_AndNeverSaves()
    {
        // Arrange
        _mockStudentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), new StudentUpdateDto
        {
            Name = "Updated",
            Major = "AI",
            GPA = 3.9
        });

        // Assert
        Assert.False(result);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesAndSaves()
    {
        // Arrange
        var id = Guid.NewGuid();
        var student = new Student("Test", "IT", 3.5) { Id = id };
        _mockStudentRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        Assert.True(result);
        _mockStudentRepo.Verify(r => r.Remove(student), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
