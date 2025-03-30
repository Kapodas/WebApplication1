using System.Text.Json.Serialization;

namespace WebApplication1.Models;

public class ProcessingResult
{
    [JsonPropertyName("noisyImage")]
    public string NoisyImageBase64 { get; set; }

    [JsonPropertyName("restoredImage")]
    public string RestoredImageBase64 { get; set; }

    [JsonPropertyName("metrics")]
    public ImageMetrics Metrics { get; set; }
}

