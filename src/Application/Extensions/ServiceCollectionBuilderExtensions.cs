using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArgbControl.Api.Application.Extensions;

public static class ServiceCollectionBuilderExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IWebSocketService, WebSocketService>();
        services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
        services.AddSingleton<IWebSocketMessageHandler, WebSocketMessageHandler>();
        services.AddSingleton<IMessageParser, MessageParser>();
        services.AddSingleton<IHashService, HashService>();
        services.AddTransient<ITokenService, TokenService>();
    }
}
