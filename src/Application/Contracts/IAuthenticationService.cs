using DataContracts;

namespace Application.Contracts;

public interface IAuthenticationService
{
    Task<string> Authenticate(string secret);
    bool IsValidToken(string token);
    WebSocketAuthInfo GetAuthInfoFromCache(string token);
}
