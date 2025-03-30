using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApplication1.Services;

public class RestorationService
{
    public Image<Rgba32> RestoreImage(Image<Rgba32> noisyImage)
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
}