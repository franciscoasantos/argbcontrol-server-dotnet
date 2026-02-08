using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.DataContracts;
using Microsoft.Extensions.Logging;

namespace ArgbControl.Api.Application.Services;

public sealed class WebSocketService(IWebSocketConnectionManager connectionManager,
                                     IWebSocketMessageHandler messageHandler,
                                     ILogger<WebSocketService> logger) : IWebSocketService
{
    public async Task StartProcessingAsync(WebSocketClient webSocketClient, CancellationToken cancellationToken)
    {
        logger.LogInformation("Socket {SocketName}: New connection from client {ClientName}",
                              webSocketClient.Socket.Name,
                              webSocketClient.Client.Name);

        connectionManager.AddConnection(webSocketClient.Socket.Id!, webSocketClient);

        await messageHandler.HandleConnectionAsync(webSocketClient, cancellationToken);
    }
}
