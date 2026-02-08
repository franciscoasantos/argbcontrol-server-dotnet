namespace ArgbControl.Api.Application.DataContracts;

public sealed class WebSocketInstance
{
    public string Id { get; }
    public byte[] Data { get; set; }
    public List<WebSocketClient> Clients { get; }

    public WebSocketInstance(string id, byte[] data, WebSocketClient client)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(client);

        Id = id;
        Data = data;
        Clients = [client];
    }
}
