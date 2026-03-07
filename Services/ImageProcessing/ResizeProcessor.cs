using ImageProcessing.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessing.Services.ImageProcessing
{
    public class ResizeProcessor : IImageProcessor
    {
        public void Process(Image image, ImageTransformOptions options)
        {
            if ((options.Width.HasValue || options.Height.HasValue) && !options.Crop)
            {
                image.Mutate(ctx =>
                {
                    ctx.Resize(options.Width ?? 0, options.Height ?? 0);
                });
            }
        }
    }
}
