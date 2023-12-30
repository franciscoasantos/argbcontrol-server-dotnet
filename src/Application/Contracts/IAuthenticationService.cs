using Application.DataContracts;

namespace Application.Contracts;

public interface IAuthenticationService
{
    Task<WebSocketAuthInfo> Authenticate(string id, string secret);
    bool TryGetAuthInfoFromCache(string id, out WebSocketAuthInfo authInfo);
}
