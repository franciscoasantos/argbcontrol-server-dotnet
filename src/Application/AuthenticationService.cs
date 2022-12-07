using Application.Contracts;
using Application.Exceptions;
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
        //TODO: Secret validation logic
        if (!secret.Equals("9a02d1e835264f6fa7f3d0ede49cea5a"))
        {
            return null!;
        }

        var token = Guid.NewGuid().ToString();

        var authInfo = Cache.GetOrCreate(token, item =>
        {
            //TODO: Client properties recovery logic
            long socketId = 0;
            long clientId = 0;
            bool isReceiver = true;

            item.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new WebSocketAuthInfo(socketId, clientId, isReceiver, token);
        });

        return authInfo!.Token;
    }

    public bool IsValidToken(string token)
    {
        var isParsed = Guid.TryParse(token, out Guid resultGuid);

        if (isParsed && Cache.TryGetValue(resultGuid.ToString(), out WebSocketAuthInfo? resultCache))
        {
            return resultCache?.Token == resultGuid.ToString();
        }

        return false;
    }

    public WebSocketAuthInfo GetAuthInfoFromCache(string token)
    {
        if (!IsValidToken(token))
        {
            throw new InvalidTokenException("Invalid token!");
        }

        Cache.TryGetValue(token, out WebSocketAuthInfo? resultCache);

        return resultCache!;
    }
}
