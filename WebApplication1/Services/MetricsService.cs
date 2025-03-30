using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace WebApplication1.Services;

public class MetricsService
{
    public double CalculatePSNR(Image<Rgba32> original, Image<Rgba32> processed)
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

    public double CalculateDataLoss(byte[] originalBytes, Image<Rgba32> processedImage)
    {
        var processedBytes = ImageToBytes(processedImage);
        return (originalBytes.Length - processedBytes.Length) / (double)originalBytes.Length * 100;
    }

    private byte[] ImageToBytes(Image<Rgba32> img)
    {
        using var ms = new MemoryStream();
        img.Save(ms, new JpegEncoder());
        return ms.ToArray();
    }
}