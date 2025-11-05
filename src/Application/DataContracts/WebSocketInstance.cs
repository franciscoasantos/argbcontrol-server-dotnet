namespace ArgbControl.Api.Application.DataContracts;

public class WebSocketInstance
{
    public string Id { get; set; }
    public byte[] Data { get; set; }
    public List<WebSocketClient> Clients { get; set; }

    public WebSocketInstance(string id, byte[] data, WebSocketClient client)
    {
        Id = id;
        Data = data;
        Clients = [client];
    }
}
