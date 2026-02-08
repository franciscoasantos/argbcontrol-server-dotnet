using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.DataContracts;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace ArgbControl.Api.Application.Services;

public sealed class WebSocketMessageHandler(IWebSocketConnectionManager connectionManager,
                                            IMessageParser messageParser,
                                            ILogger<WebSocketMessageHandler> logger) : IWebSocketMessageHandler
{
    public async Task HandleConnectionAsync(WebSocketClient client, CancellationToken cancellationToken)
    {
        try
        {
            var buffer = WebSocket.CreateServerBuffer(4096);

            if (client.Client.Roles?.Contains("sender") ?? false)
            {
                await SendInitialDataAsync(client, cancellationToken);
            }

            await ProcessMessagesAsync(client, buffer, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation(
                "Socket {SocketName}: Operation cancelled for client {ClientName}",
                client.Socket.Name,
                client.Client.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Socket {SocketName}: Error processing messages for client {ClientName}",
                client.Socket.Name,
                client.Client.Name);
        }
        finally
        {
            await CleanupConnectionAsync(client);
        }
    }

    private async Task SendInitialDataAsync(WebSocketClient client, CancellationToken cancellationToken)
    {
        if (connectionManager.TryGetSocketData(client.Socket.Id!, out var data))
        {
            await client.WebSocket.SendAsync(
                data,
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);

            logger.LogDebug(
                "Socket {SocketName}: Sent initial data to sender client {ClientName}",
                client.Socket.Name,
                client.Client.Name);
        }
    }

    private async Task ProcessMessagesAsync(
        WebSocketClient client,
        ArraySegment<byte> buffer,
        CancellationToken cancellationToken)
    {
        while (client.WebSocket.State is WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var receiveResult = await client.WebSocket.ReceiveAsync(buffer, cancellationToken);

            if (await HandleCloseMessageAsync(client, receiveResult, cancellationToken))
            {
                break;
            }

            logger.LogDebug(
                "Socket {SocketName}: Received {MessageType} frame from {ClientName} ({Count} bytes)",
                client.Socket.Name,
                receiveResult.MessageType,
                client.Client.Name,
                receiveResult.Count);

            await ProcessReceivedMessageAsync(client, buffer, receiveResult.Count, cancellationToken);
        }
    }

    private async Task<bool> HandleCloseMessageAsync(
        WebSocketClient client,
        WebSocketReceiveResult receiveResult,
        CancellationToken cancellationToken)
    {
        if (client.WebSocket.State is WebSocketState.CloseReceived && 
            receiveResult.MessageType is WebSocketMessageType.Close)
        {
            logger.LogInformation(
                "Socket {SocketName}: Acknowledging close frame from client {ClientName}",
                client.Socket.Name,
                client.Client.Name);

            await client.WebSocket.CloseOutputAsync(
                WebSocketCloseStatus.NormalClosure,
                "Acknowledge Close frame",
                cancellationToken);

            return true;
        }

        return false;
    }

    private async Task ProcessReceivedMessageAsync(
        WebSocketClient client,
        ArraySegment<byte> buffer,
        int count,
        CancellationToken cancellationToken)
    {
        var parsedData = messageParser.ParseFromJson(new ArraySegment<byte>(buffer.Array!, 0, count));
        connectionManager.UpdateSocketData(client.Socket.Id!, parsedData);

        var receiverClients = connectionManager.GetReceiverClients(client.Socket.Id!);

        if (!receiverClients.Any())
        {
            logger.LogDebug(
                "Socket {SocketName}: No receiver clients found",
                client.Socket.Name);
            return;
        }

        await BroadcastToReceiversAsync(receiverClients, parsedData, client.Socket.Name!, cancellationToken);
    }

    private async Task BroadcastToReceiversAsync(
        IEnumerable<WebSocketClient> receivers,
        byte[] data,
        string socketName,
        CancellationToken cancellationToken)
    {
        var tasks = receivers
            .Where(r => r.WebSocket.State is WebSocketState.Open)
            .Select(async receiver =>
            {
                try
                {
                    await receiver.WebSocket.SendAsync(
                        data,
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Socket {SocketName}: Failed to send message to receiver {ClientName}",
                        socketName,
                        receiver.Client.Name);
                }
            });

        await Task.WhenAll(tasks);
    }

    private async Task CleanupConnectionAsync(WebSocketClient client)
    {
        logger.LogInformation(
            "Socket {SocketName}: Ended processing for client {ClientName} in state {State}",
            client.Socket.Name,
            client.Client.Name,
            client.WebSocket.State);

        if (client.WebSocket.State is not WebSocketState.Closed)
        {
            client.WebSocket.Abort();
        }

        connectionManager.RemoveConnection(client.Socket.Id!, client);
        client.WebSocket.Dispose();
    }
}
