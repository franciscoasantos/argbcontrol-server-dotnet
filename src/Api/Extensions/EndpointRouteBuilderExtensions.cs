﻿using Api.Contracts.Request;
using Application.Contracts;
using Application.DataContracts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/token", async ([FromServices] IAuthenticationService service,
                                            [FromBody] TokenRequest request) =>
        {
            var authInfo = await service.AuthenticateAsync(request.Id, request.Secret);

            if (authInfo is null)
            {
                return Results.Unauthorized();
            }

            return Results.Json(authInfo.Token, statusCode: StatusCodes.Status200OK);
        });
    }

    public static void MapWebSocketEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.Map("/", async ([FromServices] IAuthenticationService authService,
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

            if (authService.TryGetAuthInfoFromCache(id, out WebSocketAuthInfo authInfo) is false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var wsContext = await httpContext.WebSockets.AcceptWebSocketAsync(subProtocol: null);

            var webSocketClient = new WebSocketClient(wsContext, authInfo.Socket, authInfo.Client);

            await webSocketService.StartProcessingAsync(webSocketClient, cancellationToken);
        }).RequireAuthorization("websocket");
    }
}
