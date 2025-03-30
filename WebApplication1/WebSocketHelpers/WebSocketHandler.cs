using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using WebApplication1.Models;

namespace WebApplication1.WebSocketHelpers;

public class WebSocketHandler
{
    public async Task<byte[]> ReceiveFullMessage(WebSocket ws)
    {
        using var ms = new MemoryStream();
        var buffer = new byte[1024 * 1024]; // 1MB buffer

        WebSocketReceiveResult receiveResult;
        do
        {
            receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            ms.Write(buffer, 0, receiveResult.Count);
        }
        while (!receiveResult.EndOfMessage);

        return ms.ToArray();
    }

    public async Task SendProcessingResult(
        WebSocket ws,
        Image<Rgba32> noisyImage,
        Image<Rgba32> restoredImage,
        object metricsObj)
    {
        var metrics = JsonSerializer.Deserialize<ImageMetrics>(JsonSerializer.Serialize(metricsObj));

        var result = new ProcessingResult
        {
            NoisyImageBase64 = Convert.ToBase64String(ImageToBytes(noisyImage)),
            RestoredImageBase64 = Convert.ToBase64String(ImageToBytes(restoredImage)),
            Metrics = metrics
        };

        await SendJson(ws, result);
    }

    public async Task SendError(WebSocket ws, string errorMessage)
    {
        await SendJson(ws, new { error = errorMessage });
    }

    private async Task SendJson(WebSocket ws, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    private byte[] ImageToBytes(Image<Rgba32> img)
    {
        using var ms = new MemoryStream();
        img.Save(ms, new JpegEncoder());
        return ms.ToArray();
    }
}