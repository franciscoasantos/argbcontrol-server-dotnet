using ArgbControl.Api.Application.Constants;
using ArgbControl.Api.Application.Services;
using FluentAssertions;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ArgbControl.Api.Application.Tests.Services;

public class MessageParserTests
{
    private readonly MessageParser sut = new();

    [Fact]
    public void ParseFromJson_WithRgbwMessage_ReturnsCorrectByteArray()
    {
        // Arrange
        var message = new
        {
            M = "0",
            R = "255",
            G = "128",
            B = "64",
            W = "32"
        };
        var json = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("0255128064032");
    }

    [Fact]
    public void ParseFromJson_WithArgumentsMessage_ReturnsCorrectByteArray()
    {
        // Arrange
        var message = new
        {
            M = "1",
            A = "12345"
        };
        var json = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("112345");
    }

    [Fact]
    public void ParseFromJson_WithPaddingRequired_PadsValuesCorrectly()
    {
        // Arrange
        var message = new
        {
            M = "0",
            R = "1",
            G = "2",
            B = "3",
            W = "4"
        };
        var json = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("0001002003004");
    }

    [Fact]
    public void ParseFromJson_WithArgumentsPadding_PadsArgumentsCorrectly()
    {
        // Arrange
        var message = new
        {
            M = "1",
            A = "123"
        };
        var json = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("100123");
    }

    [Fact]
    public void ParseFromJson_WithMaxValues_HandlesCorrectly()
    {
        // Arrange
        var message = new
        {
            M = "0",
            R = "255",
            G = "255",
            B = "255",
            W = "255"
        };
        var json = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("0255255255255");
    }

    [Fact]
    public void ParseFromJson_WithZeroValues_HandlesCorrectly()
    {
        // Arrange
        var message = new
        {
            M = "0",
            R = "0",
            G = "0",
            B = "0",
            W = "0"
        };
        var json = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("0000000000000");
    }

    [Fact]
    public void ParseFromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(invalidJson));

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => sut.ParseFromJson(buffer));
    }

    [Fact]
    public void ParseFromJson_WithNullMessage_ThrowsJsonException()
    {
        // Arrange
        var json = JsonSerializer.Serialize((string?)null);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sut.ParseFromJson(buffer));
    }

    [Fact]
    public void ParseFromJson_WithEmptyBuffer_ThrowsJsonException()
    {
        // Arrange
        var buffer = new ArraySegment<byte>(Array.Empty<byte>());

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => sut.ParseFromJson(buffer));
    }

    [Fact]
    public void ParseFromJson_RgbwModeWithNullValues_HandlesPaddingCorrectly()
    {
        // Arrange
        var message = new
        {
            M = "0",
            R = "0",
            G = "0",
            B = "0",
            W = "0"
        };
        var json = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("0000000000000");
    }

    [Fact]
    public void ParseFromJson_ArgumentsModeWithMaxValue_HandlesCorrectly()
    {
        // Arrange
        var message = new
        {
            M = "1",
            A = "99999"
        };
        var json = JsonSerializer.Serialize(message);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("199999");
    }

    [Fact]
    public void ParseFromJson_WithArraySegmentOffset_ParsesCorrectly()
    {
        // Arrange
        var message = new
        {
            M = "0",
            R = "255",
            G = "128",
            B = "64",
            W = "32"
        };
        var json = JsonSerializer.Serialize(message);
        var fullBuffer = Encoding.UTF8.GetBytes(json);
        var buffer = new ArraySegment<byte>(fullBuffer, 0, fullBuffer.Length);

        // Act
        var result = sut.ParseFromJson(buffer);
        var resultString = Encoding.UTF8.GetString(result);

        // Assert
        resultString.Should().Be("0255128064032");
    }
}
