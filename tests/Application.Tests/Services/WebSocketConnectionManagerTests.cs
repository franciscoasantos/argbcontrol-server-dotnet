using ArgbControl.Api.Application.Constants;
using ArgbControl.Api.Application.DataContracts;
using ArgbControl.Api.Application.Services;
using ArgbControl.Api.Infrastructure.Persistence.Models;
using FluentAssertions;
using Moq;
using System.Net.WebSockets;
using Xunit;

namespace ArgbControl.Api.Application.Tests.Services;

public class WebSocketConnectionManagerTests
{
    private readonly WebSocketConnectionManager sut = new();

    private static WebSocketClient CreateWebSocketClient(
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
    public void AddConnection_WithNewSocket_CreatesNewWebSocketInstance()
    {
        // Arrange
        var socketId = "socket-001";
        var client = CreateWebSocketClient(socketId);

        // Act
        sut.AddConnection(socketId, client);

        // Assert
        var result = sut.TryGetSocketData(socketId, out var data);
        result.Should().BeTrue();
        data.Should().BeEquivalentTo(System.Text.Encoding.ASCII.GetBytes(WebSocketConstants.DefaultInitialData));
    }

    [Fact]
    public void AddConnection_WithExistingSocket_AddsClientToExistingInstance()
    {
        // Arrange
        var socketId = "socket-001";
        var client1 = CreateWebSocketClient(socketId, "client-001", "sender");
        var client2 = CreateWebSocketClient(socketId, "client-002", "receiver");

        // Act
        sut.AddConnection(socketId, client1);
        sut.AddConnection(socketId, client2);

        // Assert
        var data = sut.TryGetSocketData(socketId, out var socketData);
        data.Should().BeTrue();
    }

    [Fact]
    public void RemoveConnection_WithExistingClient_RemovesClientFromSocket()
    {
        // Arrange
        var socketId = "socket-001";
        var client = CreateWebSocketClient(socketId, "client-001");

        sut.AddConnection(socketId, client);

        // Act
        sut.RemoveConnection(socketId, client);

        // Assert
        var receivers = sut.GetReceiverClients(socketId);
        receivers.Should().BeEmpty();
    }

    [Fact]
    public void RemoveConnection_WithNonExistentSocket_DoesNotThrow()
    {
        // Arrange
        var socketId = "non-existent";
        var client = CreateWebSocketClient(socketId);

        // Act & Assert
        sut.RemoveConnection(socketId, client);
    }

    [Fact]
    public void GetReceiverClients_WithReceiverRole_ReturnsReceiversOnly()
    {
        // Arrange
        var socketId = "socket-001";
        var senderClient = CreateWebSocketClient(socketId, "sender-client", "sender");
        var receiverClient = CreateWebSocketClient(socketId, "receiver-client", "receiver");

        sut.AddConnection(socketId, senderClient);
        sut.AddConnection(socketId, receiverClient);

        // Act
        var result = sut.GetReceiverClients(socketId);

        // Assert
        result.Should().NotBeEmpty();
        result.All(c => c.Client.Roles?.Contains(WebSocketConstants.Roles.Receiver) ?? false)
            .Should().BeTrue();
    }

    [Fact]
    public void GetReceiverClients_WithNoReceivers_ReturnsEmptyList()
    {
        // Arrange
        var socketId = "socket-001";
        var senderClient = CreateWebSocketClient(socketId, "sender-client", "sender");

        sut.AddConnection(socketId, senderClient);

        // Act
        var result = sut.GetReceiverClients(socketId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetReceiverClients_WithNonExistentSocket_ReturnsEmptyList()
    {
        // Act
        var result = sut.GetReceiverClients("non-existent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void TryGetSocketData_WithExistingSocket_ReturnsTrueAndData()
    {
        // Arrange
        var socketId = "socket-001";
        var client = CreateWebSocketClient(socketId);

        sut.AddConnection(socketId, client);

        // Act
        var result = sut.TryGetSocketData(socketId, out var data);

        // Assert
        result.Should().BeTrue();
        data.Should().NotBeEmpty();
        System.Text.Encoding.ASCII.GetString(data).Should().Be(WebSocketConstants.DefaultInitialData);
    }

    [Fact]
    public void TryGetSocketData_WithNonExistentSocket_ReturnsFalseAndEmptyData()
    {
        // Act
        var result = sut.TryGetSocketData("non-existent", out var data);

        // Assert
        result.Should().BeFalse();
        data.Should().BeEmpty();
    }

    [Fact]
    public void UpdateSocketData_WithExistingSocket_UpdatesData()
    {
        // Arrange
        var socketId = "socket-001";
        var client = CreateWebSocketClient(socketId);
        var newData = System.Text.Encoding.UTF8.GetBytes("0100150200000");

        sut.AddConnection(socketId, client);

        // Act
        sut.UpdateSocketData(socketId, newData);

        // Assert
        var result = sut.TryGetSocketData(socketId, out var data);
        result.Should().BeTrue();
        data.Should().BeEquivalentTo(newData);
    }

    [Fact]
    public void UpdateSocketData_WithNonExistentSocket_DoesNotThrow()
    {
        // Arrange
        var newData = System.Text.Encoding.UTF8.GetBytes("0100150200000");

        // Act & Assert
        sut.UpdateSocketData("non-existent", newData);
    }

    [Fact]
    public void MultipleOperations_MaintainCorrectState()
    {
        // Arrange
        var socketId = "socket-001";
        var client1 = CreateWebSocketClient(socketId, "client-001", "sender");
        var client2 = CreateWebSocketClient(socketId, "client-002", "receiver");
        var newData = System.Text.Encoding.UTF8.GetBytes("0255100050025");

        // Act
        sut.AddConnection(socketId, client1);
        sut.AddConnection(socketId, client2);
        sut.UpdateSocketData(socketId, newData);

        var dataResult = sut.TryGetSocketData(socketId, out var retrievedData);
        var receivers = sut.GetReceiverClients(socketId);

        // Assert
        dataResult.Should().BeTrue();
        retrievedData.Should().BeEquivalentTo(newData);
        receivers.Should().NotBeEmpty();
    }

    [Fact]
    public void GetReceiverClients_WithClientWithoutRoles_FiltersCorrectly()
    {
        // Arrange
        var socketId = "socket-001";
        var clientNoRole = CreateWebSocketClient(socketId, "client-no-role", null);

        sut.AddConnection(socketId, clientNoRole);

        // Act
        var result = sut.GetReceiverClients(socketId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void AddConnection_DifferentSockets_AreIndependent()
    {
        // Arrange
        var socketId1 = "socket-001";
        var socketId2 = "socket-002";
        var client1 = CreateWebSocketClient(socketId1, "client-001");
        var client2 = CreateWebSocketClient(socketId2, "client-002");

        // Act
        sut.AddConnection(socketId1, client1);
        sut.AddConnection(socketId2, client2);

        var result1 = sut.TryGetSocketData(socketId1, out var data1);
        var result2 = sut.TryGetSocketData(socketId2, out var data2);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        data1.Should().BeEquivalentTo(data2);
    }
}
