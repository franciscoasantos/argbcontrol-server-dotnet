using ArgbControl.Api.Application.Constants;
using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.DataContracts;
using ArgbControl.Api.Application.Services;
using ArgbControl.Api.Infrastructure.Persistence;
using ArgbControl.Api.Infrastructure.Persistence.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using Client = ArgbControl.Api.Application.DataContracts.Client;

namespace ArgbControl.Api.Application.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IClientsRepository> clientsRepositoryMock;
    private readonly Mock<ISocketsRepository> socketsRepositoryMock;
    private readonly Mock<IHashService> hashServiceMock;
    private readonly Mock<ITokenService> tokenServiceMock;
    private readonly IMemoryCache memoryCache;
    private readonly AuthenticationService sut;

    public AuthenticationServiceTests()
    {
        clientsRepositoryMock = new Mock<IClientsRepository>(MockBehavior.Strict);
        socketsRepositoryMock = new Mock<ISocketsRepository>(MockBehavior.Strict);
        hashServiceMock = new Mock<IHashService>(MockBehavior.Strict);
        tokenServiceMock = new Mock<ITokenService>(MockBehavior.Strict);
        memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        sut = new AuthenticationService(
            clientsRepositoryMock.Object,
            socketsRepositoryMock.Object,
            hashServiceMock.Object,
            tokenServiceMock.Object,
            memoryCache);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidIdAndSecret_ReturnsWebSocketAuthInfo()
    {
        // Arrange
        var clientId = "client-001";
        var secret = "secret-password";
        var secretHash = "hashed-secret";
        var socketId = "socket-001";
        var roles = new[] { "sender" };

        var persistenceClient = new Infrastructure.Persistence.Models.Client
        {
            Id = clientId,
            Name = "Test Client",
            SecretHash = secretHash,
            Roles = roles
        };

        var socket = new Infrastructure.Persistence.Models.Socket
        {
            Id = socketId,
            Name = "Test Socket",
            Clients = new[] { clientId }
        };

        var tokenInfo = new TokenInfo { Token = "jwt-token", ExpiresIn = 300 };
        var expectedClient = new Client(clientId, secret, roles);

        clientsRepositoryMock
            .Setup(x => x.GetAsync(It.Is<string>(id => id == clientId)))
            .ReturnsAsync(persistenceClient);

        hashServiceMock
            .Setup(x => x.IsValidHash(It.Is<string>(s => s == secret), It.Is<string>(h => h == secretHash)))
            .Returns(true);

        socketsRepositoryMock
            .Setup(x => x.GetByClientIdAsync(It.Is<string>(id => id == clientId)))
            .ReturnsAsync(socket);

        tokenServiceMock
            .Setup(x => x.Generate(It.Is<Client>(c => c.Id == clientId && c.Secret == secret)))
            .Returns(tokenInfo);

        // Act
        var result = await sut.AuthenticateAsync(clientId, secret);

        // Assert
        result.Should().NotBeNull();
        result.Socket.Id.Should().Be(socketId);
        result.Client.Id.Should().Be(clientId);
        result.Token.Token.Should().Be("jwt-token");
        result.Token.ExpiresIn.Should().Be(300);

        clientsRepositoryMock.Verify(
            x => x.GetAsync(It.Is<string>(id => id == clientId)),
            Times.Once);
        hashServiceMock.Verify(
            x => x.IsValidHash(It.Is<string>(s => s == secret), It.Is<string>(h => h == secretHash)),
            Times.Once);
        socketsRepositoryMock.Verify(
            x => x.GetByClientIdAsync(It.Is<string>(id => id == clientId)),
            Times.Once);
        tokenServiceMock.Verify(
            x => x.Generate(It.Is<Client>(c => c.Id == clientId)),
            Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidSecret_ReturnsDefault()
    {
        // Arrange
        var clientId = "client-001";
        var secret = "invalid-secret";
        var secretHash = "hashed-secret";

        var persistenceClient = new Infrastructure.Persistence.Models.Client
        {
            Id = clientId,
            Name = "Test Client",
            SecretHash = secretHash,
            Roles = new[] { "sender" }
        };

        clientsRepositoryMock
            .Setup(x => x.GetAsync(It.Is<string>(id => id == clientId)))
            .ReturnsAsync(persistenceClient);

        hashServiceMock
            .Setup(x => x.IsValidHash(It.Is<string>(s => s == secret), It.Is<string>(h => h == secretHash)))
            .Returns(false);

        // Act
        var result = await sut.AuthenticateAsync(clientId, secret);

        // Assert
        result.Should().BeNull();

        socketsRepositoryMock.Verify(
            x => x.GetByClientIdAsync(It.IsAny<string>()),
            Times.Never);
        tokenServiceMock.Verify(
            x => x.Generate(It.IsAny<Client>()),
            Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.AuthenticateAsync(null!, "secret"));
    }

    [Fact]
    public async Task AuthenticateAsync_WithEmptySecret_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.AuthenticateAsync("client-001", ""));
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentClient_ReturnsDefault()
    {
        // Arrange
        var clientId = "non-existent";
        var secret = "secret";

        clientsRepositoryMock
            .Setup(x => x.GetAsync(It.Is<string>(id => id == clientId)))
            .ReturnsAsync((Infrastructure.Persistence.Models.Client?)null);

        // Act
        var result = await sut.AuthenticateAsync(clientId, secret);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_WithNoSocket_ReturnsDefault()
    {
        // Arrange
        var clientId = "client-001";
        var secret = "secret-password";
        var secretHash = "hashed-secret";

        var persistenceClient = new Infrastructure.Persistence.Models.Client
        {
            Id = clientId,
            Name = "Test Client",
            SecretHash = secretHash,
            Roles = new[] { "sender" }
        };

        clientsRepositoryMock
            .Setup(x => x.GetAsync(It.Is<string>(id => id == clientId)))
            .ReturnsAsync(persistenceClient);

        hashServiceMock
            .Setup(x => x.IsValidHash(It.Is<string>(s => s == secret), It.Is<string>(h => h == secretHash)))
            .Returns(true);

        socketsRepositoryMock
            .Setup(x => x.GetByClientIdAsync(It.Is<string>(id => id == clientId)))
            .ReturnsAsync((Infrastructure.Persistence.Models.Socket?)null);

        // Act
        var result = await sut.AuthenticateAsync(clientId, secret);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryGetAuthInfoFromCache_WithValidId_ReturnsTrueAndAuthInfo()
    {
        // Arrange
        var clientId = "client-001";
        var cacheKey = CacheKeys.GetAuthenticationKey(clientId);
        var authInfo = new WebSocketAuthInfo(
            new Infrastructure.Persistence.Models.Socket { Id = "socket-001" },
            new Infrastructure.Persistence.Models.Client { Id = clientId },
            new TokenInfo { Token = "token", ExpiresIn = 300 });

        memoryCache.Set(cacheKey, authInfo);

        // Act
        var result = sut.TryGetAuthInfoFromCache(clientId, out var retrievedAuthInfo);

        // Assert
        result.Should().BeTrue();
        retrievedAuthInfo.Should().NotBeNull();
        retrievedAuthInfo.Client.Id.Should().Be(clientId);
    }

    [Fact]
    public void TryGetAuthInfoFromCache_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = sut.TryGetAuthInfoFromCache("non-existent", out var authInfo);

        // Assert
        result.Should().BeFalse();
        authInfo.Should().BeNull();
    }

    [Fact]
    public void TryGetAuthInfoFromCache_WithNullId_ReturnsFalse()
    {
        // Act
        var result = sut.TryGetAuthInfoFromCache(null!, out var authInfo);

        // Assert
        result.Should().BeFalse();
        authInfo.Should().BeNull();
    }

    [Fact]
    public void TryGetAuthInfoFromCache_WithEmptyId_ReturnsFalse()
    {
        // Act
        var result = sut.TryGetAuthInfoFromCache("", out var authInfo);

        // Assert
        result.Should().BeFalse();
        authInfo.Should().BeNull();
    }
}
