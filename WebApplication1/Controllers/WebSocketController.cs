using WebApplication1.Services;
using WebApplication1.WebSocketHelpers;

namespace WebApplication1.Controllers;

public class WebSocketController
{
    private readonly ImageProcessingService _imageProcessingService;
    private readonly WebSocketHandler _webSocketHandler;

    public WebSocketController(
        ImageProcessingService imageProcessingService,
        WebSocketHandler webSocketHandler)
    {
        _imageProcessingService = imageProcessingService;
        _webSocketHandler = webSocketHandler;
    }

    public async Task HandleWebSocket(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            await _imageProcessingService.ProcessImage(ws);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
}