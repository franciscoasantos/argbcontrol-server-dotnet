using ArgbControl.Api.Application.Constants;
using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.DataContracts;
using ArgbControl.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Memory;

namespace ArgbControl.Api.Application.Services;

public sealed class AuthenticationService(IClientsRepository clientsRepository,
                                          ISocketsRepository socketsRepository,
                                          IHashService hashService,
                                          ITokenService tokenService,
                                          IMemoryCache cache) : IAuthenticationService
{
    public async Task<WebSocketAuthInfo> AuthenticateAsync(string id, string secret)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Client ID cannot be null or empty", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("Secret cannot be null or empty", nameof(secret));
        }

        var client = await clientsRepository.GetAsync(id);

        if (client is null || !hashService.IsValidHash(secret, client.SecretHash!))
        {
            return default!;
        }

        var socket = await socketsRepository.GetByClientIdAsync(id);

        if (socket is null)
        {
            return default!;
        }

        var tokenInfo = tokenService.Generate(new Client(id, secret, client.Roles!));

        return cache.GetOrCreate(CacheKeys.GetAuthenticationKey(id), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenInfo.ExpiresIn);
            return new WebSocketAuthInfo(socket, client, tokenInfo);
        })!;
    }

    public bool TryGetAuthInfoFromCache(string id, out WebSocketAuthInfo authInfo)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            authInfo = default!;
            return false;
        }

        return cache.TryGetValue(CacheKeys.GetAuthenticationKey(id), out authInfo!);
    }
}
