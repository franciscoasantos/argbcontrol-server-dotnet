using Application.Contracts;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServiceCollectionBuilderExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IWebSocketService, WebSocketService>();
        services.AddSingleton<IHashService, HashService>();
        services.AddTransient<ITokenService, TokenService>();
    }
}
