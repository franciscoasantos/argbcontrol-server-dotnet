using DataContracts;

namespace Application.Contracts;

public interface IWebSocketApplicationServices
{
    Task StartProcessingAsync(WebSocketClient webSocketClient, CancellationToken cancellationToken);
}
