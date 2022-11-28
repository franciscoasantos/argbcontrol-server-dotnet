using Application;
using Application.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();

builder.Services.AddMemoryCache();
builder.Services.AddControllers();

var app = builder.Build();

app.UseWebSockets();

app.UseAuthorization();

app.MapControllers();

app.Run();
