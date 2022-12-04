using Application.Contracts;
using Application.Exceptions;
using DataContracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Controllers;

[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;
    private IWebSocketService ApplicationServices { get; set; }
    private IAuthenticationService AuthenticationService { get; set; }

    public WebSocketController(ILogger<WebSocketController> logger, IWebSocketService applicationServices, IAuthenticationService authenticationService)
    {
        _logger = logger;
        ApplicationServices = applicationServices;
        AuthenticationService = authenticationService;
    }

    [HttpGet("/")]
    public async Task WebSocketEntry([FromQuery] string token, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (!AuthenticationService.IsValidToken(token))
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            var socketId = AuthenticationService.GetSocketId(token);
            var clientId = AuthenticationService.GetClientId(token);
            var isReceiver = AuthenticationService.IsReceiver(token);

            var wsContext = await HttpContext.WebSockets.AcceptWebSocketAsync(subProtocol: null);

            var webSocketClient = new WebSocketClient(wsContext, socketId, clientId, isReceiver);

            await Task.Run(()
                => ApplicationServices.StartProcessingAsync(webSocketClient, cancellationToken), cancellationToken)
            .ConfigureAwait(true);
        }
        catch (InvalidTokenException ex)
        {
            HttpContext.Response.StatusCode = 400;
            _logger.LogError(ex.Message);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            _logger.LogError(ex.Message);
        }

    }
}
