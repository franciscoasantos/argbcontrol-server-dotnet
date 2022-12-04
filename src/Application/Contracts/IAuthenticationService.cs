using DataContracts;

namespace Application.Contracts;

public interface IAuthenticationService
{
    Task<string> Authenticate(string secret);
    bool IsValidToken(string token);
    long GetSocketId(string token);
    long GetClientId(string token);
    bool IsReceiver(string token);
}
