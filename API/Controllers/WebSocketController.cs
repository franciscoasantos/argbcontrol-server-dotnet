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
    public async Task Get(CancellationToken cancellationToken)
    {
        try
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var socketId = Guid.Parse(HttpContext.Request.Query["socketId"]);
                var isReceiver = HttpContext.Request.Query["isReceiver"].Equals("1");

                var wsContext = await HttpContext.WebSockets.AcceptWebSocketAsync(subProtocol: null);

                var webSocketClient = new WebSocketClient(wsContext, socketId, isReceiver);

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
