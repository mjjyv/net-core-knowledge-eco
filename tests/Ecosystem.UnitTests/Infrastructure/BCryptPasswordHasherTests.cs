using Ecosystem.Infrastructure.Security;

namespace Ecosystem.UnitTests.Infrastructure;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_GeneratesSaltedBCryptHash_DifferentForSameInput()
    {
        // Arrange
        const string plain = "SuperSecure123!";

        // Act
        var hash1 = _hasher.HashPassword(plain);
        var hash2 = _hasher.HashPassword(plain);

        // Assert
        Assert.StartsWith("$2", hash1);
        Assert.NotEqual(plain, hash1);
        Assert.NotEqual(hash1, hash2); // BCrypt sinh salt ngẫu nhiên mỗi lần hash
    }

    [Fact]
    public void VerifyPassword_CorrectAndIncorrectPasswords_ReturnsExpectedResult()
    {
        // Arrange
        var hash = _hasher.HashPassword("Admin@123456");

        // Act & Assert
        Assert.True(_hasher.VerifyPassword("Admin@123456", hash));
        Assert.False(_hasher.VerifyPassword("WrongPassword", hash));
        Assert.False(_hasher.VerifyPassword("", hash));
        Assert.False(_hasher.VerifyPassword("Admin@123456", "not-a-bcrypt-hash"));
    }
}
