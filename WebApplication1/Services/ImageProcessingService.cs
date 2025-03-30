using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Net.WebSockets;
using WebApplication1.Models;
using WebApplication1.WebSocketHelpers;
namespace WebApplication1.Services;


public class ImageProcessingService
{
    private readonly NoiseService _noiseService;
    private readonly RestorationService _restorationService;
    private readonly MetricsService _metricsService;
    private readonly WebSocketHandler _webSocketHandler;

    public ImageProcessingService(
        NoiseService noiseService,
        RestorationService restorationService,
        MetricsService metricsService,
        WebSocketHandler webSocketHandler)
    {
        _noiseService = noiseService;
        _restorationService = restorationService;
        _metricsService = metricsService;
        _webSocketHandler = webSocketHandler;
    }

    public async Task ProcessImage(WebSocket ws)
    {
        try
        {
            // 1. Receive image
            var imageBytes = await _webSocketHandler.ReceiveFullMessage(ws);
            using var originalImage = Image.Load<Rgba32>(imageBytes);

            // 2. Add noise
            var noisyImage = _noiseService.AddImpulseNoise(originalImage.Clone(), 0.05f);

            // 3. Restore image
            var restoredImage = _restorationService.RestoreImage(noisyImage.Clone());

            // 4. Calculate metrics
            var metrics = new ImageMetrics
            {
                PsnrNoisy = _metricsService.CalculatePSNR(originalImage, noisyImage),
                PsnrRestored = _metricsService.CalculatePSNR(originalImage, restoredImage),
                DataLossPercentage = _metricsService.CalculateDataLoss(imageBytes, restoredImage)
            };

            // 5. Send results
            await _webSocketHandler.SendProcessingResult(ws, noisyImage, restoredImage, metrics);
        }
        catch (Exception ex)
        {
            await _webSocketHandler.SendError(ws, ex.Message);
        }
    }
}