using ImageProcessing.DTOs;

namespace ImageProcessing.Services
{
    public interface IImageTransformService
    {
        Task<Stream> TransformAsync(string filePath, ImageTransformOptions options);
    }
}
