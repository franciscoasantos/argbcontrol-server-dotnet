using Microsoft.Extensions.DependencyInjection;

namespace ArgbControl.Api.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionBuilderExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IClientsRepository, ClientsRepository>();
        services.AddSingleton<ISocketsRepository, SocketsRepository>();
    }
}
