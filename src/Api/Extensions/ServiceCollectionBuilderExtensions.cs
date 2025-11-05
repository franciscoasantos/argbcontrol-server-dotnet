using ArgbControl.Api.Application.Extensions;
using ArgbControl.Api.Infrastructure.Persistence.Extensions;
using ArgbControl.Api.Infrastructure.Persistence.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ArgbControl.Api.Extensions;

public static class ServiceCollectionBuilderExtensions
{
    public static void ConfigureDependencyInjection(this IServiceCollection services)
    {
        services.AddApplicationServices();
        services.AddRepositories();
    }

    public static void ConfigureApiSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:SecurityKey"]!);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorizationBuilder().AddPolicy("websocket", policy => policy.RequireRole("sender", "receiver"));
    }

    public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        => services.Configure<MongoSettings>(configuration.GetSection("MongoDatabase"));
}
