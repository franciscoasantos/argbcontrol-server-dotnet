using System.Net.WebSockets;

namespace DataContracts;

public class WebSocketClient
{
    public Guid ClientId { get; private set; }
    public Guid SocketId { get; private set; }
    public bool IsReceiver { get; set; }
    public WebSocket Socket { get; private set; }

    public WebSocketClient(WebSocket socket, Guid socketId, bool isReceiver)
    {
        ClientId = Guid.NewGuid();
        SocketId = socketId;
        IsReceiver = isReceiver;
        Socket = socket;
    }
}
