using Application.Contracts;
using Application.DataContracts;
using Infra.Persistence;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

public class AuthenticationService(IClientsRepository clientsRepository,
                                   ISocketsRepository socketsRepository,
                                   IHashService hashService,
                                   ITokenService tokenService,
                                   IMemoryCache cache) : IAuthenticationService
{
    private const string CachePrefix = "AuthenticateService:";

    public async Task<WebSocketAuthInfo> AuthenticateAsync(string id, string secret)
    {
        var client = await clientsRepository.GetAsync(id);

        if (client is null || hashService.IsValidHash(secret, client.SecretHash!) is false)
        {
            return default!;
        }

        var socket = await socketsRepository.GetByClientIdAsync(id);

        if (socket is null)
        {
            return default!;
        }

        var tokenInfo = tokenService.Generate(new Client(id.ToString(), secret, client.Roles!));

        return cache.GetOrCreate(CachePrefix + id, item =>
        {
            item.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenInfo.ExpiresIn);
            return new WebSocketAuthInfo(socket, client, tokenInfo);
        })!;
    }

    public bool TryGetAuthInfoFromCache(string id, out WebSocketAuthInfo authInfo)
        => cache.TryGetValue(CachePrefix + id, out authInfo!);
}
