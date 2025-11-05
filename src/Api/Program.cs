using ArgbControl.Api.ExceptionHandlers;
using ArgbControl.Api.Extensions;
using System.Reflection;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureDependencyInjection();
builder.Services.ConfigureApiSecurity(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddMongoDb(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Permitir qualquer origem e qualquer método (CORS liberado)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
}

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.QueryString.HasValue && string.IsNullOrWhiteSpace(context.Request.Headers.Authorization))
    {
        var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.Value);
        var token = queryString.Get("access_token");

        if (string.IsNullOrWhiteSpace(token) is false)
            context.Request.Headers.Append("Authorization", new[] { $"Bearer {token}" });
    }

    await next.Invoke();
});

// Ativar CORS antes de autenticação
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.MapAuthEndpoints();
app.MapWebSocketEndpoints();

app.UseExceptionHandler(opt => { });

app.Run();
