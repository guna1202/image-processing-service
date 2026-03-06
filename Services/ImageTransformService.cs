using global::ImageProcessing.DTOs;
using ImageProcessing.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

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

            switch (options.Format)
            {
                case "png":
                    await image.SaveAsPngAsync(outputStream);
                    break;

                case "webp":
                    await image.SaveAsWebpAsync(outputStream);
                    break;

                default:
                    await image.SaveAsJpegAsync(outputStream);
                    break;
            }

            outputStream.Position = 0;

            return outputStream;
        }

        private void ApplyTransformations(Image image, ImageTransformOptions options)
        {
            image.Mutate(ctx =>
            {
                if (options.Width.HasValue || options.Height.HasValue)
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
                $"{imageId}_{options.Width}_{options.Height}_{options.Rotate}_{options.Grayscale}_{options.Flip}_{options.Format}_{options.Quality}";

            using var sha = System.Security.Cryptography.SHA256.Create();

            var bytes = System.Text.Encoding.UTF8.GetBytes(rawKey);

            var hash = sha.ComputeHash(bytes);

            return Convert.ToHexString(hash).ToLower();
        }
    }
}
