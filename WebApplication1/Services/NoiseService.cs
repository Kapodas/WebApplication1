using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApplication1.Services;

public class NoiseService
{
    public Image<Rgba32> AddImpulseNoise(Image<Rgba32> image, float probability)
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
}