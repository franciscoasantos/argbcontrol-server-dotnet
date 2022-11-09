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

    private static async Task StartProcessingLoopAsync(WebSocketClient client, CancellationToken cancellationToken)
    {
        try
        {
            var buffer = WebSocket.CreateServerBuffer(4096);

            var clientSocket = Sockets.FirstOrDefault(w => w.SocketId == client.SocketId);
            var receiverClients = clientSocket!.Clients.Where(w => w.IsReceiver);

            await client.Socket.SendAsync(clientSocket.Data,
                                          WebSocketMessageType.Text,
                                          true,
                                          cancellationToken);

            while (client.Socket.State != WebSocketState.Closed && client.Socket.State != WebSocketState.Aborted && !cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var receiveResult = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer.Array!), cancellationToken);

                if (client.Socket.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"Socket {client.SocketId}: Acknowledging Close frame received from client");
                    await client.Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", cancellationToken);
                    break;
                }

                Console.WriteLine($"Socket {client.SocketId}: Received {receiveResult.MessageType} frame ({receiveResult.Count} bytes).");

                clientSocket.Data = ParseMessage(new ArraySegment<byte>(buffer.Array!, 0, receiveResult.Count));

                if (receiverClients == null || !receiverClients.Any())
                {
                    Console.WriteLine($"Socket {client.SocketId}: Receiver client not found.");
                    continue;
                }

                foreach (var receiverClient in receiverClients)
                {
                    await receiverClient.Socket.SendAsync(clientSocket.Data,
                                                          WebSocketMessageType.Text,
                                                          true,
                                                          cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Socket {client.SocketId}: Operation cancelled!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Socket {client.SocketId}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Socket {client.SocketId}: Ended processing loop in state {client.Socket.State} for client: {client.ClientId}");

            if (client.Socket.State != WebSocketState.Closed)
            {
                client.Socket.Abort();
            }

            var sockets = Sockets.FirstOrDefault(w => w.SocketId == client.SocketId);

            if (sockets != null)
            {
                sockets.Clients.RemoveAll(s => s.ClientId == client.ClientId);
            }

            client.Socket.Dispose();
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
