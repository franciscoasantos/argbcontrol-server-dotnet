using ArgbControl.Api.ExceptionHandlers;
using ArgbControl.Api.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);
ConfigureConfiguration(builder);

var app = builder.Build();

ConfigureMiddleware(app);
ConfigureEndpoints(app);

app.Run();

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.ConfigureDependencyInjection();
    builder.Services.ConfigureApiSecurity(builder.Configuration);
    builder.Services.AddMemoryCache();
    builder.Services.AddMongoDb(builder.Configuration);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

static void ConfigureConfiguration(WebApplicationBuilder builder)
{
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
    }
}

static void ConfigureMiddleware(WebApplication app)
{
    app.Use(async (context, next) =>
    {
        if (context.Request.QueryString.HasValue && 
            string.IsNullOrWhiteSpace(context.Request.Headers.Authorization))
        {
            var accessToken = context.Request.Query["access_token"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");
            }
        }

        await next.Invoke();
    });

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseWebSockets();
    app.UseExceptionHandler(_ => { });
}

static void ConfigureEndpoints(WebApplication app)
{
    app.MapAuthEndpoints();
    app.MapWebSocketEndpoints();
}

