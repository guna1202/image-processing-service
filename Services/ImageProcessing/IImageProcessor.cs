using ImageProcessing.DTOs;
using SixLabors.ImageSharp;

namespace ImageProcessing.Services.ImageProcessing
{
    public interface IImageProcessor
    {
        void Process(Image image, ImageTransformOptions options);
    }
}
