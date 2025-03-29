using System.Net.WebSockets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        await ProcessImage(ws);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

async Task ProcessImage(WebSocket ws)
{
    try
    {
        // 1. Receive image
        var imageBytes = await ReceiveFullMessage(ws);
        using var originalImage = Image.Load<Rgba32>(imageBytes);

        // 2. Add noise
        var noisyImage = AddImpulseNoise(originalImage.Clone(), 0.05f);

        // 3. Restore image
        var restoredImage = RestoreImage(noisyImage.Clone());

        // 4. Calculate metrics
        var metrics = new
        {
            psnrNoisy = CalculatePSNR(originalImage, noisyImage),
            psnrRestored = CalculatePSNR(originalImage, restoredImage),
            dataLoss = (imageBytes.Length - ImageToBytes(restoredImage).Length) / (double)imageBytes.Length * 100
        };

        // 5. Send results
        await SendJson(ws, new
        {
            noisyImage = Convert.ToBase64String(ImageToBytes(noisyImage)),
            restoredImage = Convert.ToBase64String(ImageToBytes(restoredImage)),
            metrics
        });
    }
    catch (Exception ex)
    {
        await SendJson(ws, new { error = ex.Message });
    }
}

Image<Rgba32> AddImpulseNoise(Image<Rgba32> image, float probability)
{
    var rnd = new Random();
    var result = image.Clone();

    result.ProcessPixelRows(accessor =>
    {
        for (int y = 0; y < accessor.Height; y++)
        {
            var row = accessor.GetRowSpan(y);
            for (int x = 0; x < row.Length; x++)
            {
                if (rnd.NextDouble() < probability / 2)
                    row[x] = new Rgba32(255, 255, 255); // White
                else if (rnd.NextDouble() < probability / 2)
                    row[x] = new Rgba32(0, 0, 0); // Black
            }
        }
    });

    return result;
}

Image<Rgba32> RestoreImage(Image<Rgba32> noisyImage)
{
    var result = new Image<Rgba32>(noisyImage.Width, noisyImage.Height);
    var radius = 1;

    for (int y = 0; y < noisyImage.Height; y++)
    {
        for (int x = 0; x < noisyImage.Width; x++)
        {
            var pixels = new List<Rgba32>();

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    var ny = Math.Clamp(y + dy, 0, noisyImage.Height - 1);
                    var nx = Math.Clamp(x + dx, 0, noisyImage.Width - 1);
                    pixels.Add(noisyImage[nx, ny]);
                }
            }

            pixels.Sort((a, b) => (a.R + a.G + a.B).CompareTo(b.R + b.G + b.B));
            result[x, y] = pixels[pixels.Count / 2];
        }
    }

    return result;
}

double CalculatePSNR(Image<Rgba32> original, Image<Rgba32> processed)
{
    if (original.Width != processed.Width || original.Height != processed.Height)
        throw new ArgumentException("Images must have the same dimensions");

    double mse = 0;

    for (int y = 0; y < original.Height; y++)
    {
        for (int x = 0; x < original.Width; x++)
        {
            var origPixel = original[x, y];
            var procPixel = processed[x, y];
            mse += Math.Pow(origPixel.R - procPixel.R, 2) +
                   Math.Pow(origPixel.G - procPixel.G, 2) +
                   Math.Pow(origPixel.B - procPixel.B, 2);
        }
    }

    mse /= (original.Width * original.Height * 3);
    return 20 * Math.Log10(255 / Math.Sqrt(mse));
}

byte[] ImageToBytes(Image<Rgba32> img)
{
    using var ms = new MemoryStream();
    img.Save(ms, new JpegEncoder());
    return ms.ToArray();
}

async Task<byte[]> ReceiveFullMessage(WebSocket ws)
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

async Task SendJson(WebSocket ws, object data)
{
    var json = JsonSerializer.Serialize(data);
    var bytes = Encoding.UTF8.GetBytes(json);
    await ws.SendAsync(
        new ArraySegment<byte>(bytes),
        WebSocketMessageType.Text,
        true,
        CancellationToken.None);
}

app.MapGet("/", () => "Hello World!");
app.Run();