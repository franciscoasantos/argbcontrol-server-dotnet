using Infra.Persistence.Models;

namespace Application.DataContracts;

public class WebSocketAuthInfo(Socket socket, Infra.Persistence.Models.Client client, TokenInfo token)
{
    public Socket Socket { get; set; } = socket;
    public Infra.Persistence.Models.Client Client { get; set; } = client;
    public TokenInfo Token { get; set; } = token;
}
