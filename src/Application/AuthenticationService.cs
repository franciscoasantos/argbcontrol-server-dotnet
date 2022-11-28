using Application.Contracts;
using DataContracts;
using Microsoft.Extensions.Caching.Memory;

namespace Application;

public class AuthenticationService : IAuthenticationService
{
    private readonly IMemoryCache Cache;

    public AuthenticationService(IMemoryCache cache)
    {
        Cache = cache;
    }

    public async Task<string> Authenticate(string secret)
    {
        if (!secret.Equals("9a02d1e835264f6fa7f3d0ede49cea5a"))
        {
            return null!;
        }

        var authToken = Guid.NewGuid();

        var authInfo = Cache.GetOrCreate(authToken, item =>
        {
            item.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new WebSocketAuthInfo(authToken);
        });

        return authInfo!.AuthToken.ToString();

    }

    public bool IsValidToken(string token)
    {
        var isParsed = Guid.TryParse(token, out Guid resultGuid);

        if (isParsed && Cache.TryGetValue(resultGuid, out WebSocketAuthInfo? resultCache))
        {
            return resultCache?.AuthToken == resultGuid;
        }

        return false;
    }
}
