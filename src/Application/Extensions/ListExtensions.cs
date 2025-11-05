using ArgbControl.Api.Application.DataContracts;

namespace ArgbControl.Api.Application.Extensions;

public static class ListExtensions
{
    public static void AddOrUpdate(this IList<WebSocketInstance> currentSockets, WebSocketInstance newSocket)
    {
        var socket = currentSockets.FirstOrDefault(w => w.Id == newSocket.Id);

        if (socket is null)
        {
            currentSockets.Add(newSocket);
        }
        else
        {
            socket.Clients.AddRange(newSocket.Clients);
        }
    }
}
