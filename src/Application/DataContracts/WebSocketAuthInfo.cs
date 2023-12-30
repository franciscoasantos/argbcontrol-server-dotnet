using Infra.Persistence.Models;

namespace Application.DataContracts;

public class WebSocketAuthInfo
{
    public Socket Socket { get; set; }
    public Infra.Persistence.Models.Client Client { get; set; }
    public string Token { get; set; }

    public WebSocketAuthInfo(Socket socket, Infra.Persistence.Models.Client client, string token)
    {
        Socket = socket;
        Client = client;
        Token = token;
    }
}
