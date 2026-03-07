using global::ImageProcessing.DTOs;
using ImageProcessing.DTOs;
using ImageProcessing.Services.ImageProcessing;
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
        private readonly IEnumerable<IImageProcessor> _processors;

        public ImageTransformService(IEnumerable<IImageProcessor> processors)
        {
            _processors = processors;
        }

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
            foreach (var processor in _processors)
            {
                processor.Process(image, options);
            }
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
