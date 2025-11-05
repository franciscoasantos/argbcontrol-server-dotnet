using ArgbControl.Api.Infrastructure.Persistence.Models;
using System.Net.WebSockets;

namespace ArgbControl.Api.Application.DataContracts;

public sealed class WebSocketClient
{
    public WebSocket WebSocket { get; private set; }
    public Socket Socket { get; private set; }
    public Infrastructure.Persistence.Models.Client Client { get; private set; }

    public WebSocketClient(WebSocket webSocket, Socket socket, Infrastructure.Persistence.Models.Client client)
    {
        Client = client;
        Socket = socket;
        WebSocket = webSocket;
    }
}