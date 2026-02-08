using ArgbControl.Api.Application.DataContracts;

namespace ArgbControl.Api.Application.Contracts;

public interface IWebSocketConnectionManager
{
    void AddConnection(string socketId, WebSocketClient client);
    void RemoveConnection(string socketId, WebSocketClient client);
    IEnumerable<WebSocketClient> GetReceiverClients(string socketId);
    bool TryGetSocketData(string socketId, out byte[] data);
    void UpdateSocketData(string socketId, byte[] data);
}
