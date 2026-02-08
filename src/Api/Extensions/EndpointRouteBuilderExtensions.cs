using ArgbControl.Api.Application.Contracts;
using ArgbControl.Api.Application.DataContracts;
using ArgbControl.Api.Contracts.Request;
using Microsoft.AspNetCore.Mvc;

namespace ArgbControl.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/token", async (
            [FromServices] IAuthenticationService service,
            [FromBody] TokenRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Secret))
            {
                return Results.BadRequest(new { error = "Id and Secret are required" });
            }

            var authInfo = await service.AuthenticateAsync(request.Id, request.Secret);

            if (authInfo is null)
            {
                return Results.Unauthorized();
            }

            return Results.Json(authInfo.Token, statusCode: StatusCodes.Status200OK);
        })
        .WithName("GenerateToken");
    }

    public static void MapWebSocketEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.Map("/", async (
            [FromServices] IAuthenticationService authService,
            [FromServices] IWebSocketService webSocketService,
            [FromQuery(Name = "client_id")] string id,
            HttpContext httpContext) =>
        {
            if (httpContext.WebSockets.IsWebSocketRequest is false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                return;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (authService.TryGetAuthInfoFromCache(id, out var authInfo) is false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var wsContext = await httpContext.WebSockets.AcceptWebSocketAsync(subProtocol: null);
            var webSocketClient = new WebSocketClient(wsContext, authInfo.Socket, authInfo.Client);

            await webSocketService.StartProcessingAsync(webSocketClient, httpContext.RequestAborted);
        })
        .RequireAuthorization("websocket")
        .WithName("WebSocketEndpoint");
    }
}
