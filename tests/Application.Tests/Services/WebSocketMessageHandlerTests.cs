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

public class WebSocketMessageHandlerTests
{
    private readonly Mock<IWebSocketConnectionManager> connectionManagerMock;
    private readonly Mock<IMessageParser> messageParserMock;
    private readonly Mock<ILogger<WebSocketMessageHandler>> loggerMock;
    private readonly WebSocketMessageHandler sut;

    public WebSocketMessageHandlerTests()
    {
        connectionManagerMock = new Mock<IWebSocketConnectionManager>(MockBehavior.Strict);
        messageParserMock = new Mock<IMessageParser>(MockBehavior.Strict);
        loggerMock = new Mock<ILogger<WebSocketMessageHandler>>(MockBehavior.Strict);

        SetupDefaultMockBehavior();

        sut = new WebSocketMessageHandler(
            connectionManagerMock.Object,
            messageParserMock.Object,
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

    private static WebSocketClient CreateMockWebSocketClient(
        string socketId = "socket-001",
        string clientId = "client-001",
        string? clientRole = "sender")
    {
        var webSocketMock = new Mock<WebSocket>(MockBehavior.Strict);
        webSocketMock
            .Setup(x => x.State)
            .Returns(WebSocketState.Open);

        var socket = new Socket
        {
            Id = socketId,
            Name = "Test Socket",
            Clients = new[] { clientId }
        };

        var client = new Infrastructure.Persistence.Models.Client
        {
            Id = clientId,
            Name = "Test Client",
            Roles = clientRole is not null ? new[] { clientRole } : Array.Empty<string>()
        };

        return new WebSocketClient(webSocketMock.Object, socket, client);
    }

    [Fact]
    public async Task HandleConnectionAsync_WithSenderClient_SendsInitialData()
    {
        // Arrange
        var webSocketMock = new Mock<WebSocket>(MockBehavior.Strict);
        var initialData = System.Text.Encoding.UTF8.GetBytes("0000000000255");
        var stateSequence = new[] { WebSocketState.Open, WebSocketState.Open, WebSocketState.Open };
        var stateIndex = 0;

        webSocketMock
            .Setup(x => x.State)
            .Returns(() =>
            {
                var state = stateSequence[Math.Min(stateIndex++, stateSequence.Length - 1)];
                return state;
            });

        webSocketMock
            .Setup(x => x.SendAsync(
                It.Is<ArraySegment<byte>>(ab => ab.SequenceEqual(initialData)),
                It.Is<WebSocketMessageType>(wm => wm == WebSocketMessageType.Text),
                It.Is<bool>(b => b == true),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        webSocketMock
            .Setup(x => x.ReceiveAsync(
                It.IsAny<ArraySegment<byte>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(50);
                return new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
            });

        webSocketMock
            .Setup(x => x.Abort());

        webSocketMock
            .Setup(x => x.Dispose());

        var socket = new Socket { Id = "socket-001", Name = "Test Socket" };
        var client = new Infrastructure.Persistence.Models.Client { Id = "client-001", Roles = new[] { "sender" } };
        var webSocketClient = new WebSocketClient(webSocketMock.Object, socket, client);

        connectionManagerMock
            .Setup(x => x.TryGetSocketData(
                It.Is<string>(id => id == "socket-001"),
                out It.Ref<byte[]>.IsAny))
            .Returns((string id, out byte[] data) =>
            {
                data = initialData;
                return true;
            });

        connectionManagerMock
            .Setup(x => x.RemoveConnection(
                It.IsAny<string>(),
                It.IsAny<WebSocketClient>()));

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        // Act
        await sut.HandleConnectionAsync(webSocketClient, cts.Token);

        // Assert
        webSocketMock.Verify(
            x => x.SendAsync(
                It.Is<ArraySegment<byte>>(ab => ab.SequenceEqual(initialData)),
                It.Is<WebSocketMessageType>(wm => wm == WebSocketMessageType.Text),
                It.Is<bool>(b => b == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleConnectionAsync_WithReceiverClient_DoesNotSendInitialData()
    {
        // Arrange
        var webSocketMock = new Mock<WebSocket>(MockBehavior.Strict);
        var stateSequence = new[] { WebSocketState.Open, WebSocketState.Open, WebSocketState.Open };
        var stateIndex = 0;

        webSocketMock
            .Setup(x => x.State)
            .Returns(() =>
            {
                var state = stateSequence[Math.Min(stateIndex++, stateSequence.Length - 1)];
                return state;
            });

        webSocketMock
            .Setup(x => x.ReceiveAsync(
                It.IsAny<ArraySegment<byte>>(),
                It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(50);
                return new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
            });

        webSocketMock
            .Setup(x => x.Abort());

        webSocketMock
            .Setup(x => x.Dispose());

        var socket = new Socket { Id = "socket-001", Name = "Test Socket" };
        var client = new Infrastructure.Persistence.Models.Client { Id = "client-001", Roles = new[] { "receiver" } };
        var webSocketClient = new WebSocketClient(webSocketMock.Object, socket, client);

        connectionManagerMock
            .Setup(x => x.RemoveConnection(
                It.IsAny<string>(),
                It.IsAny<WebSocketClient>()));

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        // Act
        await sut.HandleConnectionAsync(webSocketClient, cts.Token);

        // Assert
        webSocketMock.Verify(
            x => x.SendAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<WebSocketMessageType>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleConnectionAsync_WithCancelledToken_StopsProcessing()
    {
        // Arrange
        var webSocketMock = new Mock<WebSocket>(MockBehavior.Strict);

        webSocketMock
            .Setup(x => x.State)
            .Returns(WebSocketState.Open);

        webSocketMock
            .Setup(x => x.Abort());

        webSocketMock
            .Setup(x => x.Dispose());

        var socket = new Socket { Id = "socket-001", Name = "Test Socket" };
        var client = new Infrastructure.Persistence.Models.Client { Id = "client-001" };
        var webSocketClient = new WebSocketClient(webSocketMock.Object, socket, client);

        connectionManagerMock
            .Setup(x => x.RemoveConnection(
                It.IsAny<string>(),
                It.IsAny<WebSocketClient>()));

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await sut.HandleConnectionAsync(webSocketClient, cts.Token);

        // Assert
        webSocketMock.Verify(
            x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleConnectionAsync_WithWebSocketException_LogsError()
    {
        // Arrange
        var webSocketMock = new Mock<WebSocket>(MockBehavior.Strict);
        var exception = new IOException("Connection lost");

        webSocketMock
            .Setup(x => x.State)
            .Returns(WebSocketState.Open);

        webSocketMock
            .Setup(x => x.ReceiveAsync(
                It.IsAny<ArraySegment<byte>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        webSocketMock
            .Setup(x => x.Abort());

        webSocketMock
            .Setup(x => x.Dispose());

        var socket = new Socket { Id = "socket-001", Name = "Test Socket" };
        var client = new Infrastructure.Persistence.Models.Client { Id = "client-001" };
        var webSocketClient = new WebSocketClient(webSocketMock.Object, socket, client);

        connectionManagerMock
            .Setup(x => x.RemoveConnection(
                It.IsAny<string>(),
                It.IsAny<WebSocketClient>()));

        var cts = new CancellationTokenSource();

        // Act
        await sut.HandleConnectionAsync(webSocketClient, cts.Token);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleConnectionAsync_WithValidData_ProcessesSuccessfully()
    {
        // Arrange
        var webSocketMock = new Mock<WebSocket>(MockBehavior.Strict);
        var messageData = System.Text.Encoding.UTF8.GetBytes("test message");
        var callCount = 0;

        webSocketMock
            .Setup(x => x.State)
            .Returns(() => callCount > 2 ? WebSocketState.Closed : WebSocketState.Open);

        webSocketMock
            .Setup(x => x.ReceiveAsync(
                It.IsAny<ArraySegment<byte>>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    var buffer = new byte[4096];
                    messageData.CopyTo(buffer, 0);
                    return Task.FromResult(new WebSocketReceiveResult(messageData.Length, WebSocketMessageType.Text, true));
                }
                return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Text, true));
            });

        webSocketMock
            .Setup(x => x.Abort());

        webSocketMock
            .Setup(x => x.Dispose());

        var socket = new Socket { Id = "socket-001", Name = "Test Socket" };
        var client = new Infrastructure.Persistence.Models.Client { Id = "client-001" };
        var webSocketClient = new WebSocketClient(webSocketMock.Object, socket, client);

        messageParserMock
            .Setup(x => x.ParseFromJson(It.Is<ArraySegment<byte>>(ab => ab.Count == messageData.Length)))
            .Returns(new byte[] { 0, 1, 2, 3 });

        connectionManagerMock
            .Setup(x => x.RemoveConnection(
                It.IsAny<string>(),
                It.IsAny<WebSocketClient>()));

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        // Act
        await sut.HandleConnectionAsync(webSocketClient, cts.Token);

        // Assert
        webSocketMock.Verify(
            x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}
