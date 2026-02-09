using ArgbControl.Api.Application.DataContracts;
using ArgbControl.Api.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using Client = ArgbControl.Api.Application.DataContracts.Client;

namespace ArgbControl.Api.Application.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> configurationMock;
    private readonly TokenService sut;

    public TokenServiceTests()
    {
        configurationMock = new Mock<IConfiguration>(MockBehavior.Strict);
        SetupConfiguration();
        sut = new TokenService(configurationMock.Object);
    }

    private void SetupConfiguration()
    {
        var jwtSettings = new Mock<IConfigurationSection>(MockBehavior.Strict);
        jwtSettings
            .Setup(x => x.Value)
            .Returns("your-secret-key-must-be-at-least-32-characters-long!");

        configurationMock
            .Setup(x => x[It.Is<string>(s => s == "Jwt:SecurityKey")])
            .Returns("your-secret-key-must-be-at-least-32-characters-long!");
    }

    [Fact]
    public void Generate_WithValidClient_ReturnsValidTokenInfo()
    {
        // Arrange
        var client = new Client("client-001", "secret", new[] { "sender" });

        // Act
        var result = sut.Generate(client);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresIn.Should().BeGreaterThan(0);
        result.ExpiresIn.Should().BeLessThanOrEqualTo(300);
    }

    [Fact]
    public void Generate_WithValidClient_TokenCanBeDecoded()
    {
        // Arrange
        var client = new Client("client-001", "secret", new[] { "sender", "receiver" });
        var handler = new JwtSecurityTokenHandler();

        // Act
        var result = sut.Generate(client);
        var token = handler.ReadJwtToken(result.Token);

        // Assert
        token.Should().NotBeNull();
        token.Claims.Should().NotBeEmpty();
    }

    [Fact]
    public void Generate_WithValidClient_ContainsClientIdInClaims()
    {
        // Arrange
        var clientId = "client-001";
        var client = new Client(clientId, "secret", new[] { "sender" });
        var handler = new JwtSecurityTokenHandler();

        // Act
        var result = sut.Generate(client);
        var token = handler.ReadJwtToken(result.Token);

        // Assert
        var nameClaim = token.Claims.FirstOrDefault(c => c.Type == "unique_name");
        nameClaim.Should().NotBeNull();
        nameClaim!.Value.Should().Be(clientId);
    }

    [Fact]
    public void Generate_WithClientRoles_ContainsRolesInClaims()
    {
        // Arrange
        var roles = new[] { "sender", "receiver", "admin" };
        var client = new Client("client-001", "secret", roles);
        var handler = new JwtSecurityTokenHandler();

        // Act
        var result = sut.Generate(client);
        var token = handler.ReadJwtToken(result.Token);

        // Assert
        var roleClaims = token.Claims.Where(c => c.Type == "role").ToList();
        roleClaims.Should().HaveCount(3);
        roleClaims.Select(c => c.Value).Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void Generate_WithClientWithoutRoles_ContainsNoRoleClaims()
    {
        // Arrange
        var client = new Client("client-001", "secret", Array.Empty<string>());
        var handler = new JwtSecurityTokenHandler();

        // Act
        var result = sut.Generate(client);
        var token = handler.ReadJwtToken(result.Token);

        // Assert
        var roleClaims = token.Claims.Where(c => c.Type == "role");
        roleClaims.Should().BeEmpty();
    }

    [Fact]
    public void Generate_WithMultipleClients_ProduceDifferentTokens()
    {
        // Arrange
        var client1 = new Client("client-001", "secret", new[] { "sender" });
        var client2 = new Client("client-002", "secret", new[] { "receiver" });

        // Act
        var token1 = sut.Generate(client1).Token;
        var token2 = sut.Generate(client2).Token;

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void Generate_ExpiresInShouldBeApproximately5Minutes()
    {
        // Arrange
        var client = new Client("client-001", "secret", new[] { "sender" });
        var expectedExpirationSeconds = 300;

        // Act
        var result = sut.Generate(client);

        // Assert
        result.ExpiresIn.Should().BeCloseTo(expectedExpirationSeconds, 5);
    }

    [Fact]
    public void Generate_TokenExpirationIsSet()
    {
        // Arrange
        var client = new Client("client-001", "secret", new[] { "sender" });
        var handler = new JwtSecurityTokenHandler();

        // Act
        var result = sut.Generate(client);
        var token = handler.ReadJwtToken(result.Token);

        // Assert
        token.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Generate_WithNullClient_ThrowsNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => sut.Generate(null!));
    }

    [Fact]
    public void Generate_WithSpecialCharactersInClientId_GeneratesValidToken()
    {
        // Arrange
        var client = new Client("client-@#$%_001", "secret", new[] { "sender" });
        var handler = new JwtSecurityTokenHandler();

        // Act
        var result = sut.Generate(client);
        var token = handler.ReadJwtToken(result.Token);

        // Assert
        token.Should().NotBeNull();
        var nameClaim = token.Claims.FirstOrDefault(c => c.Type == "unique_name");
        nameClaim!.Value.Should().Be("client-@#$%_001");
    }
}
