using ArgbControl.Api.Application.Constants;
using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.DataContracts;
using System.Collections.Concurrent;
using System.Text;

namespace ArgbControl.Api.Application.Services;

public sealed class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocketInstance> sockets = new();
    private readonly Lock @lock = new();

    public void AddConnection(string socketId, WebSocketClient client)
    {
        sockets.AddOrUpdate(
            socketId,
            _ => new WebSocketInstance(
                socketId, 
                Encoding.ASCII.GetBytes(WebSocketConstants.DefaultInitialData), 
                client),
            (_, existing) =>
            {
                lock (@lock)
                {
                    existing.Clients.Add(client);
                }
                return existing;
            });
    }

    public void RemoveConnection(string socketId, WebSocketClient client)
    {
        if (sockets.TryGetValue(socketId, out var socketInstance))
        {
            lock (@lock)
            {
                socketInstance.Clients.RemoveAll(c => 
                    c.Client.Id == client.Client.Id && 
                    c.WebSocket.State == System.Net.WebSockets.WebSocketState.Aborted);
            }
        }
    }

    public IEnumerable<WebSocketClient> GetReceiverClients(string socketId)
    {
        if (sockets.TryGetValue(socketId, out var socketInstance))
        {
            lock (@lock)
            {
                return [.. socketInstance.Clients.Where(c => c.Client.Roles?.Contains(WebSocketConstants.Roles.Receiver) ?? false)];
            }
        }

        return [];
    }

    public bool TryGetSocketData(string socketId, out byte[] data)
    {
        if (sockets.TryGetValue(socketId, out var socketInstance))
        {
            data = socketInstance.Data;
            return true;
        }

        data = [];
        return false;
    }

    public void UpdateSocketData(string socketId, byte[] data)
    {
        if (sockets.TryGetValue(socketId, out var socketInstance))
        {
            socketInstance.Data = data;
        }
    }
}
