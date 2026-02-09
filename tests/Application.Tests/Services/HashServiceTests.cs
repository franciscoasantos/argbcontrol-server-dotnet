using ArgbControl.Api.Application.Services;
using FluentAssertions;
using Xunit;

namespace ArgbControl.Api.Application.Tests.Services;

public class HashServiceTests
{
    private readonly HashService sut = new();

    [Fact]
    public void GenerateHash_WithValidInput_ReturnsNonEmptyString()
    {
        // Arrange
        var rawData = "my-secret-password";

        // Act
        var hash = sut.GenerateHash(rawData);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().Contain(":");
    }

    [Fact]
    public void GenerateHash_SameInputProducesDifferentHashes()
    {
        // Arrange
        var rawData = "my-secret-password";

        // Act
        var hash1 = sut.GenerateHash(rawData);
        var hash2 = sut.GenerateHash(rawData);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void IsValidHash_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var rawData = "my-secret-password";
        var hashedData = sut.GenerateHash(rawData);

        // Act
        var result = sut.IsValidHash(rawData, hashedData);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidHash_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var rawData = "my-secret-password";
        var wrongData = "different-password";
        var hashedData = sut.GenerateHash(rawData);

        // Act
        var result = sut.IsValidHash(wrongData, hashedData);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidHash_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        var rawData = "my-secret-password";
        var hashedData = sut.GenerateHash(rawData);

        // Act
        var result = sut.IsValidHash("", hashedData);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidHash_WithCorruptedHash_ThrowsFormatException()
    {
        // Arrange
        var hashedData = "invalid:corrupted:hash";

        // Act & Assert
        Assert.Throws<FormatException>(() => sut.IsValidHash("password", hashedData));
    }

    [Fact]
    public void IsValidHash_WithHashFormatWithoutColon_ThrowsFormatException()
    {
        // Arrange
        var hashedData = "invalidhashnocolon";

        // Act & Assert
        Assert.Throws<FormatException>(() => sut.IsValidHash("password", hashedData));
    }

    [Fact]
    public void IsValidHash_WithMultipleColonsInHash_ProcessesSuccessfully()
    {
        // Arrange
        var rawData = "test:password:with:colons";
        var hashedData = sut.GenerateHash(rawData);

        // Act
        var result = sut.IsValidHash(rawData, hashedData);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidHash_WithSpecialCharactersInPassword_WorksCorrectly()
    {
        // Arrange
        var rawData = "p@ssw0rd!#$%&*()_+-=[]{}|;:',.<>?/";
        var hashedData = sut.GenerateHash(rawData);

        // Act
        var result = sut.IsValidHash(rawData, hashedData);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GenerateHash_WithUnicodeCharacters_ReturnsValidHash()
    {
        // Arrange
        var rawData = "senha-café-???";

        // Act
        var hash = sut.GenerateHash(rawData);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().Contain(":");
    }

    [Fact]
    public void IsValidHash_WithUnicodeCharacters_ReturnsTrue()
    {
        // Arrange
        var rawData = "senha-café-???";
        var hashedData = sut.GenerateHash(rawData);

        // Act
        var result = sut.IsValidHash(rawData, hashedData);

        // Assert
        result.Should().BeTrue();
    }
}
