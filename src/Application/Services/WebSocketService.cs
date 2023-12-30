using Application.Contracts;
using Application.DataContracts;
using Application.Extensions;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Application.Services;

public class WebSocketService : IWebSocketService
{
    private static readonly List<WebSocketInstance> Sockets = [];

    public async Task StartProcessingAsync(WebSocketClient webSocketClient, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Socket {webSocketClient.Socket.Name}: New connection ({webSocketClient.Client.Name}).");

        Sockets.AddOrUpdate(new WebSocketInstance(webSocketClient.Socket.Id!,
                                                  Encoding.ASCII.GetBytes("200010"),
                                                  webSocketClient));

        await StartProcessingLoopAsync(webSocketClient, cancellationToken);
    }

    private static async Task StartProcessingLoopAsync(WebSocketClient client, CancellationToken cancellationToken)
    {
        try
        {
            var buffer = WebSocket.CreateServerBuffer(4096);

            var clientSocket = Sockets.FirstOrDefault(w => w.Id == client.Socket.Id);
            var receiverClients = clientSocket!.Clients.Where(w => w.Client.Roles!.Any(w => w == "receiver"));

            await client.WebSocket.SendAsync(clientSocket.Data,
                                          WebSocketMessageType.Text,
                                          true,
                                          cancellationToken);

            while (client.WebSocket.State != WebSocketState.Closed && client.WebSocket.State != WebSocketState.Aborted && !cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var receiveResult = await client.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer.Array!), cancellationToken);

                if (client.WebSocket.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"Socket {client.Socket.Name}: Acknowledging Close frame received from client {client.Client.Name}");
                    await client.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", cancellationToken);
                    break;
                }

                Console.WriteLine($"Socket {client.Socket.Name}: Received {receiveResult.MessageType} frame from {client.Client.Name} ({receiveResult.Count} bytes).");

                clientSocket.Data = ParseMessage(new ArraySegment<byte>(buffer.Array!, 0, receiveResult.Count));

                if (receiverClients == null || !receiverClients.Any())
                {
                    Console.WriteLine($"Socket {client.Socket.Name}: Receiver clients not found.");
                    continue;
                }

                foreach (var receiverClient in receiverClients)
                {
                    await receiverClient.WebSocket.SendAsync(clientSocket.Data,
                                                             WebSocketMessageType.Text,
                                                             true,
                                                             cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Socket {client.Socket.Name}: Operation cancelled!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Socket {client.Socket.Name}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Socket {client.Socket.Name}: Ended processing loop in state {client.WebSocket.State} for client {client.Client.Name}");

            if (client.WebSocket.State != WebSocketState.Closed)
            {
                client.WebSocket.Abort();
            }

            var sockets = Sockets.FirstOrDefault(w => w.Id == client.Socket.Id);

            sockets?.Clients.RemoveAll(s => s.Client == client.Client && s.WebSocket.State == WebSocketState.Aborted);

            client.WebSocket.Dispose();
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
