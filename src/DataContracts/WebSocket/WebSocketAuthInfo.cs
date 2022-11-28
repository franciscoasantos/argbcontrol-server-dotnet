namespace DataContracts;

public class WebSocketAuthInfo
{
    public Guid AuthToken { get; set; }

    public WebSocketAuthInfo(Guid authToken)
    {
        AuthToken = authToken;
    }
}
