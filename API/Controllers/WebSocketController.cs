using Application.Contracts;
using DataContracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace api.Controllers;

[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;
    private IWebSocketApplicationServices ApplicationServices { get; set; }

    public WebSocketController(ILogger<WebSocketController> logger, IWebSocketApplicationServices applicationServices)
    {
        _logger = logger;
        ApplicationServices = applicationServices;
    }

    [HttpGet("/")]
    public async Task WebSocketEntry([FromQuery] Guid socketId,
                                     [FromQuery] Guid clientId = default,
                                     [FromQuery] bool isReceiver = false,
                                     CancellationToken cancellationToken = default)
    {
        try
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var wsContext = await HttpContext.WebSockets.AcceptWebSocketAsync(subProtocol: null);

                var webSocketClient = new WebSocketClient(wsContext, socketId, isReceiver, clientId);

                await Task.Run(()
                    => ApplicationServices.StartProcessingAsync(webSocketClient, cancellationToken), cancellationToken)
                .ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = 500;
            _logger.LogError(ex.Message);
        }

    }
}
