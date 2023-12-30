using Infra.Persistence.Models;
using System.Net.WebSockets;

namespace Application.DataContracts;

public sealed class WebSocketClient
{
    public WebSocket WebSocket { get; private set; }
    public Socket Socket { get; private set; }
    public Infra.Persistence.Models.Client Client { get; private set; }

    public WebSocketClient(WebSocket webSocket, Socket socket, Infra.Persistence.Models.Client client)
    {
        Client = client;
        Socket = socket;
        WebSocket = webSocket;
    }
}