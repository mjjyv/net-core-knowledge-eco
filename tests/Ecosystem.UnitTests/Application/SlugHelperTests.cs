using Ecosystem.Application.Common;

namespace Ecosystem.UnitTests.Application;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Kiến Trúc Clean Architecture Với .NET 8", "kien-truc-clean-architecture-voi-net-8")]
    [InlineData("Lập Trình C# Nâng Cao & EF Core!", "lap-trinh-c-nang-cao-ef-core")]
    [InlineData("Hướng dẫn sử dụng LINQ từ cơ bản đến nâng cao", "huong-dan-su-dung-linq-tu-co-ban-den-nang-cao")]
    [InlineData("Định nghĩa và Phân biệt Đệ quy", "dinh-nghia-va-phan-biet-de-quy")]
    [InlineData("   Khoảng   trắng   dư thừa   ", "khoang-trang-du-thua")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void GenerateSlug_TransformsVietnameseText_ToKebabCase(string input, string expected)
    {
        // Act
        var result = SlugHelper.GenerateSlug(input);

        // Assert
        Assert.Equal(expected, result);
    }
}
