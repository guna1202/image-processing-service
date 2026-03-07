using ImageProcessing.DTOs;

namespace ImageProcessing.Services
{
    public class ImageProcessingJob
    {
        private readonly IImageTransformService _imageTransformService;

        public ImageProcessingJob(IImageTransformService imageTransformService)
        {
            _imageTransformService = imageTransformService;
        }

        public async Task ProcessTransformJob(string filePath, ImageTransformOptions options, string cacheFilePath)
        {
            var stream = await _imageTransformService.TransformAsync(filePath, options);

            using var fileStream = new FileStream(cacheFilePath, FileMode.Create);

            await stream.CopyToAsync(fileStream);
        }
    }
}
