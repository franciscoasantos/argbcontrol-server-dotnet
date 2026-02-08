using ArgbControl.Api.Infrastructure.Persistence.Models;
using System.Net.WebSockets;

namespace ArgbControl.Api.Application.DataContracts;

public sealed class WebSocketClient(WebSocket webSocket, Socket socket, Infrastructure.Persistence.Models.Client client)
{
    public WebSocket WebSocket { get; } = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
    public Socket Socket { get; } = socket ?? throw new ArgumentNullException(nameof(socket));
    public Infrastructure.Persistence.Models.Client Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
}