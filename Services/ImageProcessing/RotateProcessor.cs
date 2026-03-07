using ImageProcessing.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessing.Services.ImageProcessing
{
    public class RotateProcessor : IImageProcessor
    {
        public void Process(Image image, ImageTransformOptions options)
        {
            if (options.Rotate.HasValue)
            {
                image.Mutate(ctx => ctx.Rotate(options.Rotate.Value));
            }
        }
    }
}
