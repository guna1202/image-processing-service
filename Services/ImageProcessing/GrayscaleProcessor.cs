using ImageProcessing.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessing.Services.ImageProcessing
{
    public class GrayscaleProcessor : IImageProcessor
    {
        public void Process(Image image, ImageTransformOptions options)
        {
            if (options.Grayscale)
            {
                image.Mutate(ctx => ctx.Grayscale());
            }
        }
    }
}
