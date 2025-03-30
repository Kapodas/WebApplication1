using System.Text.Json.Serialization;

namespace WebApplication1.Models;

public class ImageMetrics
{
    [JsonPropertyName("psnrNoisy")]
    public double PsnrNoisy { get; set; }

    [JsonPropertyName("psnrRestored")]
    public double PsnrRestored { get; set; }

    [JsonPropertyName("dataLoss")]
    public double DataLossPercentage { get; set; }
}

