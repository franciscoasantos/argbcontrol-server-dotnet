using ArgbControl.Api.Infrastructure.Persistence.Models;

namespace ArgbControl.Api.Application.DataContracts;

public sealed class WebSocketAuthInfo(Socket socket, Infrastructure.Persistence.Models.Client client, TokenInfo token)
{
    public Socket Socket { get; } = socket ?? throw new ArgumentNullException(nameof(socket));
    public Infrastructure.Persistence.Models.Client Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
    public TokenInfo Token { get; } = token ?? throw new ArgumentNullException(nameof(token));
}
