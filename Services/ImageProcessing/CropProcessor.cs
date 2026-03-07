using ImageProcessing.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessing.Services.ImageProcessing
{
    public class CropProcessor : IImageProcessor
    {
        public void Process(Image image, ImageTransformOptions options)
        {
            if (options.Crop && options.Width.HasValue && options.Height.HasValue)
            {
                var cropWidth = options.Width.Value;
                var cropHeight = options.Height.Value;

                var x = (image.Width - cropWidth) / 2;
                var y = (image.Height - cropHeight) / 2;

                image.Mutate(ctx =>
                {
                    ctx.Crop(new Rectangle(x, y, cropWidth, cropHeight));
                });
            }
        }
    }
}