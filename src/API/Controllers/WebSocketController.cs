using Application.Contracts;
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
    public async Task WebSocketEntry([FromQuery] Guid socketId,
                                     [FromQuery] string token,
                                     [FromQuery] bool isReceiver = false,
                                     CancellationToken cancellationToken = default)
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

            var wsContext = await HttpContext.WebSockets.AcceptWebSocketAsync(subProtocol: null);

            var webSocketClient = new WebSocketClient(wsContext, socketId, isReceiver);

            await Task.Run(()
                => ApplicationServices.StartProcessingAsync(webSocketClient, cancellationToken), cancellationToken)
            .ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            _logger.LogError(ex.Message);
        }

    }
}
