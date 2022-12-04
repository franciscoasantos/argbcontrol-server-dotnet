namespace DataContracts;

public class WebSocketAuthInfo
{ 
    public long SocketId { get; set; }
    public long ClientId { get; set; }
    public bool IsReceiver { get; set; }
    public string Token { get; set; }

    public WebSocketAuthInfo(long socketId, long clientId, bool isReceiver, string token)
    {
        SocketId = socketId;
        ClientId = clientId;
        IsReceiver = isReceiver;
        Token = token;
    }
}
