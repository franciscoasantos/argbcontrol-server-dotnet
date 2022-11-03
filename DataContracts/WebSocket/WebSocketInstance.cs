namespace DataContracts;

public class WebSocketInstance
{
    public Guid SocketId { get; set; }
    public byte[] Data { get; set; }
    public List<WebSocketClient> Clients { get; set; }

    public WebSocketInstance(Guid socketId, byte[] data, WebSocketClient client)
    {
        SocketId = socketId;
        Data = data;
        Clients = new List<WebSocketClient> { client };
    }
}
