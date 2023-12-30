using Application.Contracts;
using Application.DataContracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/token", async ([FromServices] IAuthenticationService authenticationService,
                                           [FromHeader(Name = "X-Client-Id")] string id,
                                           [FromHeader(Name = "X-Client-Secret")] string secret) =>
        {
            var authInfo = await authenticationService.Authenticate(id, secret);

            return authInfo is null
                ? Results.Unauthorized()
                : Results.Text(authInfo.Token, statusCode: StatusCodes.Status200OK);
        });
    }

    public static void MapWebSocketEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.Map("/", async ([FromServices] IAuthenticationService authenticationService,
                                [FromServices] IWebSocketService webSocketService,
                                [FromQuery(Name = "client_id")] string id,
                                HttpContext httpContext,
                                CancellationToken cancellationToken) =>
        {
            if (httpContext.WebSockets.IsWebSocketRequest is false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
                return;
            }

            if (authenticationService.TryGetAuthInfoFromCache(id, out WebSocketAuthInfo authInfo) is false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var wsContext = await httpContext.WebSockets.AcceptWebSocketAsync(subProtocol: null);

            var webSocketClient = new WebSocketClient(wsContext,
                                                      authInfo.Socket,
                                                      authInfo.Client);

            await webSocketService.StartProcessingAsync(webSocketClient, cancellationToken);
        }).RequireAuthorization("websocket");
    }
}
