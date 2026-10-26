using Ecosystem.Application.Services;
using Ecosystem.Domain.Entities;
using Xunit;

namespace Ecosystem.UnitTests.Application;

public class StudentManagerTests
{
    private readonly StudentManager _manager = new();

    [Fact]
    public async Task GetAllAsync_ShouldReturnInitialStudents()
    {
        // Act
        var students = await _manager.GetAllAsync();

        // Assert
        Assert.NotNull(students);
        Assert.True(students.Count >= 5, "Danh sách ban đầu phải có ít nhất 5 sinh viên.");
    }

    [Fact]
    public async Task FilterByMajorAndGpaAsync_ShouldReturnMatchingStudentsOrderedByGpa()
    {
        // Act: Lọc sinh viên ngành CNTT có GPA >= 3.0
        var results = await _manager.FilterByMajorAndGpaAsync("CNTT", 3.0);

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, s =>
        {
            Assert.Equal("CNTT", s.Major);
            Assert.True(s.GPA >= 3.0);
        });

        // Kiểm tra thứ tự giảm dần theo GPA
        var gpaList = results.Select(s => s.GPA).ToList();
        var sortedList = gpaList.OrderByDescending(g => g).ToList();
        Assert.Equal(sortedList, gpaList);
    }

    [Fact]
    public async Task GetTopStudentsAsync_ShouldReturnTopNStudentsWithHighestGpa()
    {
        // Act: Lấy 3 sinh viên điểm cao nhất
        var top3 = await _manager.GetTopStudentsAsync(3);

        // Assert
        Assert.Equal(3, top3.Count);
        Assert.True(top3[0].GPA >= top3[1].GPA);
        Assert.True(top3[1].GPA >= top3[2].GPA);
        // "Chi Lê" có GPA 3.9 là cao nhất trong danh sách mẫu
        Assert.Equal("Chi Lê", top3[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdExists_ShouldReturnStudent()
    {
        // Arrange
        var all = await _manager.GetAllAsync();
        var expected = all.First();

        // Act
        var actual = await _manager.GetByIdAsync(expected.Id);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var randomId = Guid.NewGuid();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _manager.GetByIdAsync(randomId));
        Assert.Contains(randomId.ToString(), ex.Message);
    }

    [Fact]
    public async Task AddStudentAsync_ShouldIncreaseTotalCount()
    {
        // Arrange
        var initialList = await _manager.GetAllAsync();
        int initialCount = initialList.Count;

        var newStudent = new Student("Vũ Minh", "CNTT", 3.85);

        // Act
        await _manager.AddStudentAsync(newStudent);
        var updatedList = await _manager.GetAllAsync();

        // Assert
        Assert.Equal(initialCount + 1, updatedList.Count);
        var found = await _manager.GetByIdAsync(newStudent.Id);
        Assert.Equal("Vũ Minh", found.Name);
    }

    [Fact]
    public async Task RemoveStudentAsync_ShouldSoftDeleteStudent()
    {
        // Arrange
        var all = await _manager.GetAllAsync();
        var target = all.Last();

        // Act
        await _manager.RemoveStudentAsync(target.Id);
        var remainingList = await _manager.GetAllAsync();

        // Assert
        Assert.DoesNotContain(remainingList, s => s.Id == target.Id);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _manager.GetByIdAsync(target.Id));
    }
}
