using Application;
using Application.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IWebSocketApplicationServices, WebSocketApplicationServices>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseWebSockets();

app.UseAuthorization();

app.MapControllers();

app.Run();
