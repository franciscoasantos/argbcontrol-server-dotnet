using Application.Contracts;
using Application.Extensions;
using DataContracts;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Application;

public class WebSocketApplicationServices : IWebSocketApplicationServices
{
    private static readonly List<WebSocketInstance> Sockets = new();

    public async Task StartProcessingAsync(WebSocketClient webSocketClient, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Socket {webSocketClient.SocketId}: New connection.");

        Sockets.AddOrUpdate(new WebSocketInstance(webSocketClient.SocketId,
                                                  Encoding.ASCII.GetBytes("200010"),
                                                  webSocketClient));

        await StartProcessingLoopAsync(webSocketClient, cancellationToken);
    }

    private static async Task StartProcessingLoopAsync(WebSocketClient currentClient, CancellationToken cancellationToken)
    {
        var socketClient = currentClient.Socket;

        try
        {
            var buffer = WebSocket.CreateServerBuffer(4096);

            var currentSocket = Sockets.FirstOrDefault(w => w.SocketId == currentClient.SocketId);
            var receiverClients = currentSocket!.Clients.Where(w => w.IsReceiver);

            await currentClient.Socket.SendAsync(currentSocket.Data,
                                                 WebSocketMessageType.Text,
                                                 true,
                                                 cancellationToken);

            while (socketClient.State != WebSocketState.Closed && socketClient.State != WebSocketState.Aborted && !cancellationToken.IsCancellationRequested)
            {
                var receiveResult = await currentClient.Socket.ReceiveAsync(new ArraySegment<byte>(buffer.Array!), cancellationToken);

                if (currentClient.Socket.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"Socket {currentClient.SocketId}: Acknowledging Close frame received from client");
                    await socketClient.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", cancellationToken);
                }

                if (currentClient.Socket.State == WebSocketState.Open)
                {
                    currentSocket.Data = ParseMessage(new ArraySegment<byte>(buffer.Array!, 0, receiveResult.Count));

                    Console.WriteLine($"Socket {currentClient.SocketId}: Received {receiveResult.MessageType} frame ({receiveResult.Count} bytes).");

                    if (receiverClients == null || !receiverClients.Any())
                    {
                        Console.WriteLine($"Socket {currentClient.SocketId}: Receiver client not found.");
                        continue;
                    }

                    foreach (var receiverClient in receiverClients)
                    {
                        await receiverClient.Socket.SendAsync(currentSocket.Data,
                                                              WebSocketMessageType.Text,
                                                              true,
                                                              cancellationToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal upon task/token cancellation, disregard
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Socket {currentClient.SocketId}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Socket {currentClient.SocketId}: Ended processing loop in state {socketClient.State} for client: {currentClient.ClientId}");

            if (currentClient.Socket.State != WebSocketState.Closed)
            {
                currentClient.Socket.Abort();
            }

            var sockets = Sockets.FirstOrDefault(w => w.SocketId == currentClient.SocketId);

            if (sockets != null)
            {
                sockets.Clients.RemoveAll(s => s.ClientId == currentClient.ClientId);
            }

            socketClient.Dispose();
        }
    }

    private static byte[] ParseMessage(ArraySegment<byte> buffer)
    {
        var jsonString = Encoding.UTF8.GetString(buffer);

        var message = JsonSerializer.Deserialize<Message>(jsonString);
        var stringMessage = message!.Mode == "0" ? message.GetRgbMessage() : message.GetArgumentsMessage();

        return Encoding.UTF8.GetBytes(stringMessage);
    }
}
