using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.DataContracts;
using ArgbControl.Api.Application.Services;
using ArgbControl.Api.Infrastructure.Persistence.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.WebSockets;
using Xunit;

namespace ArgbControl.Api.Application.Tests.Services;

public class WebSocketServiceTests
{
    private readonly Mock<IWebSocketConnectionManager> connectionManagerMock;
    private readonly Mock<IWebSocketMessageHandler> messageHandlerMock;
    private readonly Mock<ILogger<WebSocketService>> loggerMock;
    private readonly WebSocketService sut;

    public WebSocketServiceTests()
    {
        connectionManagerMock = new Mock<IWebSocketConnectionManager>(MockBehavior.Strict);
        messageHandlerMock = new Mock<IWebSocketMessageHandler>(MockBehavior.Strict);
        loggerMock = new Mock<ILogger<WebSocketService>>(MockBehavior.Strict);

        SetupDefaultMockBehavior();

        sut = new WebSocketService(
            connectionManagerMock.Object,
            messageHandlerMock.Object,
            loggerMock.Object);
    }

    private void SetupDefaultMockBehavior()
    {
        loggerMock.Setup(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));
    }

    private static WebSocketClient CreateWebSocketClient(
        string socketId = "socket-001",
        string socketName = "Test Socket",
        string clientId = "client-001",
        string clientName = "Test Client")
    {
        var webSocketMock = new Mock<WebSocket>(MockBehavior.Strict);
        webSocketMock
            .Setup(x => x.State)
            .Returns(WebSocketState.Open);

        var socket = new Socket
        {
            Id = socketId,
            Name = socketName,
            Clients = new[] { clientId }
        };

        var client = new Infrastructure.Persistence.Models.Client
        {
            Id = clientId,
            Name = clientName,
            Roles = new[] { "sender" }
        };

        return new WebSocketClient(webSocketMock.Object, socket, client);
    }

    [Fact]
    public async Task StartProcessingAsync_WithValidClient_AddsConnectionAndHandlesMessages()
    {
        // Arrange
        var webSocketClient = CreateWebSocketClient();

        connectionManagerMock
            .Setup(x => x.AddConnection(
                It.Is<string>(id => id == "socket-001"),
                It.Is<WebSocketClient>(c => c.Client.Id == "client-001")))
            .Verifiable();

        messageHandlerMock
            .Setup(x => x.HandleConnectionAsync(
                It.Is<WebSocketClient>(c => c.Client.Id == "client-001"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();

        // Act
        await sut.StartProcessingAsync(webSocketClient, cts.Token);

        // Assert
        connectionManagerMock.Verify(
            x => x.AddConnection(
                It.Is<string>(id => id == "socket-001"),
                It.IsAny<WebSocketClient>()),
            Times.Once);

        messageHandlerMock.Verify(
            x => x.HandleConnectionAsync(
                It.IsAny<WebSocketClient>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StartProcessingAsync_LogsConnectionInformation()
    {
        // Arrange
        var webSocketClient = CreateWebSocketClient(
            socketName: "RGB Socket",
            clientName: "Main Client");

        connectionManagerMock
            .Setup(x => x.AddConnection(It.IsAny<string>(), It.IsAny<WebSocketClient>()));

        messageHandlerMock
            .Setup(x => x.HandleConnectionAsync(It.IsAny<WebSocketClient>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();

        // Act
        await sut.StartProcessingAsync(webSocketClient, cts.Token);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartProcessingAsync_WithMultipleClients_ProcessesBoth()
    {
        // Arrange
        var client1 = CreateWebSocketClient("socket-001", "Socket 1", "client-001", "Client 1");
        var client2 = CreateWebSocketClient("socket-002", "Socket 2", "client-002", "Client 2");

        connectionManagerMock
            .Setup(x => x.AddConnection(It.IsAny<string>(), It.IsAny<WebSocketClient>()));

        messageHandlerMock
            .Setup(x => x.HandleConnectionAsync(It.IsAny<WebSocketClient>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();

        // Act
        await sut.StartProcessingAsync(client1, cts.Token);
        await sut.StartProcessingAsync(client2, cts.Token);

        // Assert
        connectionManagerMock.Verify(
            x => x.AddConnection(It.IsAny<string>(), It.IsAny<WebSocketClient>()),
            Times.Exactly(2));

        messageHandlerMock.Verify(
            x => x.HandleConnectionAsync(It.IsAny<WebSocketClient>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task StartProcessingAsync_WithCancelledToken_PassesTokenToHandler()
    {
        // Arrange
        var webSocketClient = CreateWebSocketClient();
        var cts = new CancellationTokenSource();

        connectionManagerMock
            .Setup(x => x.AddConnection(It.IsAny<string>(), It.IsAny<WebSocketClient>()));

        messageHandlerMock
            .Setup(x => x.HandleConnectionAsync(
                It.IsAny<WebSocketClient>(),
                It.Is<CancellationToken>(ct => ct == cts.Token)))
            .Returns(Task.CompletedTask);

        // Act
        await sut.StartProcessingAsync(webSocketClient, cts.Token);

        // Assert
        messageHandlerMock.Verify(
            x => x.HandleConnectionAsync(
                It.IsAny<WebSocketClient>(),
                It.Is<CancellationToken>(ct => ct == cts.Token)),
            Times.Once);
    }

    [Fact]
    public async Task StartProcessingAsync_WithCorrectSocketId_AddConnectionCalledWithCorrectId()
    {
        // Arrange
        var socketId = "specific-socket-id";
        var webSocketClient = CreateWebSocketClient(socketId: socketId);

        connectionManagerMock
            .Setup(x => x.AddConnection(
                It.Is<string>(id => id == socketId),
                It.IsAny<WebSocketClient>()))
            .Verifiable();

        messageHandlerMock
            .Setup(x => x.HandleConnectionAsync(It.IsAny<WebSocketClient>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();

        // Act
        await sut.StartProcessingAsync(webSocketClient, cts.Token);

        // Assert
        connectionManagerMock.Verify(
            x => x.AddConnection(
                It.Is<string>(id => id == socketId),
                It.IsAny<WebSocketClient>()),
            Times.Once);
    }

    [Fact]
    public async Task StartProcessingAsync_WithMessageHandlerThrowingException_ExceptionPropagates()
    {
        // Arrange
        var webSocketClient = CreateWebSocketClient();
        var exception = new InvalidOperationException("Handler error");

        connectionManagerMock
            .Setup(x => x.AddConnection(It.IsAny<string>(), It.IsAny<WebSocketClient>()));

        messageHandlerMock
            .Setup(x => x.HandleConnectionAsync(It.IsAny<WebSocketClient>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var cts = new CancellationTokenSource();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.StartProcessingAsync(webSocketClient, cts.Token));
    }

    [Fact]
    public async Task StartProcessingAsync_ExecutionOrder_ConnectionAddedBeforeHandling()
    {
        // Arrange
        var executionOrder = new List<string>();
        var webSocketClient = CreateWebSocketClient();

        connectionManagerMock
            .Setup(x => x.AddConnection(It.IsAny<string>(), It.IsAny<WebSocketClient>()))
            .Callback(() => executionOrder.Add("AddConnection"));

        messageHandlerMock
            .Setup(x => x.HandleConnectionAsync(It.IsAny<WebSocketClient>(), It.IsAny<CancellationToken>()))
            .Callback(async () =>
            {
                executionOrder.Add("HandleConnection");
                await Task.CompletedTask;
            })
            .Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();

        // Act
        await sut.StartProcessingAsync(webSocketClient, cts.Token);

        // Assert
        executionOrder.Should().HaveCount(2);
        executionOrder[0].Should().Be("AddConnection");
        executionOrder[1].Should().Be("HandleConnection");
    }
}
