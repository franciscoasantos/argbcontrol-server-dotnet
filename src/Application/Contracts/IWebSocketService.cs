using Application.DataContracts;

namespace Application.Contracts;

public interface IWebSocketService
{
    Task StartProcessingAsync(WebSocketClient webSocketClient, CancellationToken cancellationToken);
}
