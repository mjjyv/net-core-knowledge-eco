using System.ComponentModel.DataAnnotations;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Validation;

namespace Ecosystem.UnitTests.Application;

public class DataAnnotationValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);

        // Kích hoạt IValidatableObject nếu đối tượng có triển khai
        if (model is IValidatableObject validatable)
        {
            var customResults = validatable.Validate(context);
            if (customResults != null)
            {
                validationResults.AddRange(customResults);
            }
        }

        return validationResults;
    }

    #region UserRegisterDto Validation Tests

    [Fact]
    public void UserRegisterDto_WhenValid_PassesValidation()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "senior_dev",
            Password = "SecurePassword123",
            ConfirmPassword = "SecurePassword123",
            Email = "developer@ecosystem.vn",
            Age = 28,
            PhoneNumber = "0912345678"
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void UserRegisterDto_WhenMissingRequiredFields_FailsValidation()
    {
        // Arrange (toàn bộ các trường bắt buộc để rỗng)
        var dto = new UserRegisterDto();

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.Username)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.Password)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.ConfirmPassword)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.Email)));
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.Age)));
    }

    [Theory]
    [InlineData("a")] // Quá ngắn (< 3)
    [InlineData("this_username_is_way_too_long_for_system")] // Quá dài (> 20)
    [InlineData("john doe")] // Có khoảng trắng
    [InlineData("user@name!")] // Ký tự đặc biệt
    public void UserRegisterDto_WhenUsernameInvalidFormat_FailsValidation(string invalidUsername)
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = invalidUsername,
            Password = "SecurePassword123",
            ConfirmPassword = "SecurePassword123",
            Email = "valid@ecosystem.vn",
            Age = 25
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.Username)));
    }

    [Theory]
    [InlineData("12345")] // Quá ngắn (< 6)
    [InlineData("alllowercase123")] // Không có chữ hoa
    [InlineData("ALLUPPERCASE123")] // Không có chữ thường
    [InlineData("NoNumberAtAll")] // Không có chữ số
    public void UserRegisterDto_WhenPasswordComplexityFails_FailsValidation(string weakPassword)
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "valid_user",
            Password = weakPassword,
            ConfirmPassword = weakPassword,
            Email = "valid@ecosystem.vn",
            Age = 25
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.Password)));
    }

    [Fact]
    public void UserRegisterDto_WhenPasswordsDoNotMatch_FailsValidation()
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "valid_user",
            Password = "Password123",
            ConfirmPassword = "DifferentPassword456", // Không khớp
            Email = "valid@ecosystem.vn",
            Age = 25
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.ConfirmPassword)));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@missingusername.com")]
    [InlineData("plainaddress")]
    public void UserRegisterDto_WhenEmailInvalid_FailsValidation(string invalidEmail)
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "valid_user",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Email = invalidEmail,
            Age = 25
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.Email)));
    }

    [Theory]
    [InlineData(17)] // Dưới 18
    [InlineData(61)] // Trên 60
    [InlineData(-5)]
    public void UserRegisterDto_WhenAgeOutOfRange_FailsValidation(int invalidAge)
    {
        // Arrange
        var dto = new UserRegisterDto
        {
            Username = "valid_user",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Email = "valid@ecosystem.vn",
            Age = invalidAge
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(UserRegisterDto.Age)));
    }

    #endregion

    #region TopicCreateDto & SlugValidation Tests

    [Fact]
    public void TopicCreateDto_WhenValid_PassesValidation()
    {
        // Arrange
        var dto = new TopicCreateDto
        {
            Title = "Kiến Trúc Clean Architecture",
            Slug = "kien-truc-clean-architecture",
            Description = "Mô hình phân tầng Clean Architecture và DDD",
            OrderIndex = 1
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("Kien-Truc-Clean")] // Có chữ hoa
    [InlineData("kien truc clean")] // Có khoảng trắng
    [InlineData("kien_truc_clean")] // Có dấu gạch dưới thay vì gạch ngang
    [InlineData("kien-truc-clean!")] // Có ký tự đặc biệt
    [InlineData("-kien-truc-")] // Gạch ngang ở đầu hoặc cuối
    public void TopicCreateDto_WhenSlugInvalidKebabCase_FailsValidation(string invalidSlug)
    {
        // Arrange
        var dto = new TopicCreateDto
        {
            Title = "Tiêu Đề Hợp Lệ",
            Slug = invalidSlug,
            OrderIndex = 1
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(TopicCreateDto.Slug)));
    }

    [Fact]
    public void TopicCreateDto_WhenSlugIsReservedAdmin_FailsValidationViaIValidatableObject()
    {
        // Arrange
        var dto = new TopicCreateDto
        {
            Title = "Quản Trị Hệ Thống",
            Slug = "admin", // Từ khóa cấm
            OrderIndex = 1
        };

        // Act
        var errors = ValidateModel(dto);

        // Assert
        Assert.Contains(errors, e => e.ErrorMessage!.Contains("từ khóa hệ thống dành riêng"));
    }

    #endregion

    #region MustBeEvenAttribute Tests

    [Theory]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(42, true)]
    [InlineData(-10, true)]
    [InlineData(1, false)]
    [InlineData(3, false)]
    [InlineData(99, false)]
    [InlineData(-7, false)]
    public void MustBeEvenAttribute_ValidatesCorrectly(int value, bool expected)
    {
        // Arrange
        var attribute = new MustBeEvenAttribute();

        // Act
        var isValid = attribute.IsValid(value);

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Fact]
    public void MustBeEvenAttribute_WhenNull_ReturnsTrue()
    {
        // Arrange
        var attribute = new MustBeEvenAttribute();

        // Act
        var isValid = attribute.IsValid(null);

        // Assert
        Assert.True(isValid);
    }

    #endregion
}
