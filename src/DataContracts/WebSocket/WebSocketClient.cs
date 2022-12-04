using System.Net.WebSockets;

namespace DataContracts;

public class WebSocketClient
{
    public long ClientId { get; private set; }
    public long SocketId { get; private set; }
    public bool IsReceiver { get; set; }
    public WebSocket Socket { get; private set; }

    public WebSocketClient(WebSocket socket, long socketId, long clientId, bool isReceiver)
    {
        ClientId = clientId;
        SocketId = socketId;
        IsReceiver = isReceiver;
        Socket = socket;
    }
}