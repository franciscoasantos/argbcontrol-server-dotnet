using Application.Contracts;
using Application.Exceptions;
using DataContracts;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> WebSocketEntry([FromQuery] string token, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                return BadRequest();
            }

            if (!AuthenticationService.IsValidToken(token))
            {
                return Unauthorized();
            }

            var socketId = AuthenticationService.GetSocketId(token);
            var clientId = AuthenticationService.GetClientId(token);
            var isReceiver = AuthenticationService.IsReceiver(token);

            var wsContext = await HttpContext.WebSockets.AcceptWebSocketAsync(subProtocol: null);

            var webSocketClient = new WebSocketClient(wsContext, socketId, clientId, isReceiver);

            await Task.Run(()
                => ApplicationServices.StartProcessingAsync(webSocketClient, cancellationToken), cancellationToken)
            .ConfigureAwait(true);

            return Ok();
        }
        catch (InvalidTokenException ex)
        {
            _logger.LogError(ex.Message);

            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            return Problem(ex.Message);
        }

    }
}
