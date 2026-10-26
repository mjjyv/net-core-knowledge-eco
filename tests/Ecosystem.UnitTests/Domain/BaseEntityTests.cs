using Ecosystem.Domain.Common;
using Ecosystem.Domain.Entities;
using Xunit;

namespace Ecosystem.UnitTests.Domain;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity
    {
    }

    [Fact]
    public void BaseEntity_WhenInstantiated_ShouldHaveValidDefaultValues()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.True(entity.CreatedAt <= DateTime.UtcNow);
        Assert.True(entity.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        Assert.Null(entity.UpdatedAt);
        Assert.False(entity.IsDeleted);
    }

    [Fact]
    public void ApiResponse_Ok_ShouldReturnSuccessWithData()
    {
        // Arrange
        const string payload = "Hello Clean Architecture";

        // Act
        var response = ApiResponse<string>.Ok(payload, "Thành công");

        // Assert
        Assert.True(response.Success);
        Assert.Equal(payload, response.Data);
        Assert.Equal("Thành công", response.Message);
        Assert.Null(response.Errors);
    }

    [Fact]
    public void ApiResponse_Fail_ShouldReturnFailureWithErrors()
    {
        // Act
        var response = ApiResponse<string>.Fail("Lỗi xảy ra", new List<string> { "Chi tiết 1", "Chi tiết 2" });

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Count);
    }
}
