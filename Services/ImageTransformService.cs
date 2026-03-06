using global::ImageProcessing.DTOs;
using ImageProcessing.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text;

namespace ImageProcessing.Services
{
    public class ImageTransformService : IImageTransformService
    {
        public async Task<Stream> TransformAsync(string filePath, ImageTransformOptions options)
        {
            using var imageStream = File.OpenRead(filePath);

            var image = await Image.LoadAsync(imageStream);

            ApplyTransformations(image, options);

            var outputStream = new MemoryStream();

            var quality = options.Quality ?? 90;

            switch (options.Format?.ToLower())
            {
                case "png":
                    await image.SaveAsPngAsync(outputStream);
                    break;

                case "webp":
                    var webpEncoder = new WebpEncoder
                    {
                        Quality = quality
                    };
                    await image.SaveAsWebpAsync(outputStream, webpEncoder);
                    break;

                default:
                    var jpegEncoder = new JpegEncoder
                    {
                        Quality = quality
                    };
                    await image.SaveAsJpegAsync(outputStream, jpegEncoder);
                    break;
            }

            outputStream.Position = 0;

            return outputStream;
        }

        private void ApplyTransformations(Image image, ImageTransformOptions options)
        {
            image.Mutate(ctx =>
            {
                if (options.Crop && options.Width.HasValue && options.Height.HasValue)
                {
                    var cropWidth = options.Width.Value;
                    var cropHeight = options.Height.Value;

                    if (cropWidth > image.Width || cropHeight > image.Height)
                    {
                        cropWidth = image.Width;
                        cropHeight = image.Height;
                    }

                    var x = (image.Width - cropWidth) / 2;
                    var y = (image.Height - cropHeight) / 2;

                    ctx.Crop(new Rectangle(x, y, cropWidth, cropHeight));
                }
                else if (options.Width.HasValue || options.Height.HasValue)
                {
                    ctx.Resize(options.Width ?? 0, options.Height ?? 0);
                }

                if (options.Rotate.HasValue)
                {
                    ctx.Rotate(options.Rotate.Value);
                }

                if (options.Grayscale)
                {
                    ctx.Grayscale();
                }

                if (options.Flip)
                {
                    ctx.Flip(FlipMode.Vertical);
                }
            });
        }

        public string GenerateCacheKey(Guid imageId, ImageTransformOptions options)
        {
            var rawKey =
                $"{imageId}_{options.Width}_{options.Height}_{options.Crop}_{options.Rotate}_{options.Grayscale}_{options.Flip}_{options.Format}_{options.Quality}";

            using var sha = SHA256.Create();

            var bytes = Encoding.UTF8.GetBytes(rawKey);

            var hash = sha.ComputeHash(bytes);

            return Convert.ToHexString(hash).ToLower();
        }
    }
}
