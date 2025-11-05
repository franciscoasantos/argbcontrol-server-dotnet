using ArgbControl.Api.Application.DataContracts;

namespace ArgbControl.Api.Application.Contracts;

public interface IAuthenticationService
{
    Task<WebSocketAuthInfo> AuthenticateAsync(string id, string secret);
    bool TryGetAuthInfoFromCache(string id, out WebSocketAuthInfo authInfo);
}
