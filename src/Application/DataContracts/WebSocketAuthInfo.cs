using ArgbControl.Api.Infrastructure.Persistence.Models;

namespace ArgbControl.Api.Application.DataContracts;

public class WebSocketAuthInfo(Socket socket, Infrastructure.Persistence.Models.Client client, TokenInfo token)
{
    public Socket Socket { get; set; } = socket;
    public Infrastructure.Persistence.Models.Client Client { get; set; } = client;
    public TokenInfo Token { get; set; } = token;
}
