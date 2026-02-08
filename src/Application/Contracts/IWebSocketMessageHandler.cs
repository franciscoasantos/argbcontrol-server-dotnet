using ArgbControl.Api.Application.DataContracts;

namespace ArgbControl.Api.Application.Contracts;

public interface IWebSocketMessageHandler
{
    Task HandleConnectionAsync(WebSocketClient client, CancellationToken cancellationToken);
}
