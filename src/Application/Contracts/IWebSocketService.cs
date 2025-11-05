using ArgbControl.Api.Application.DataContracts;

namespace ArgbControl.Api.Application.Contracts;

public interface IWebSocketService
{
    Task StartProcessingAsync(WebSocketClient webSocketClient, CancellationToken cancellationToken);
}
