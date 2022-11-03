using DataContracts;

namespace Application.Extensions;

public static class ListExtensions
{
    public static void AddOrUpdate(this IList<WebSocketInstance> currentSockets, WebSocketInstance newSocket)
    {
        var socket = currentSockets.FirstOrDefault(w => w.SocketId == newSocket.SocketId);

        if (socket == null)
        {
            currentSockets.Add(newSocket);
        }
        else
        {
            socket.Clients.AddRange(newSocket.Clients);
        }
    }
}
