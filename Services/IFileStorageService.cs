using ImageProcessing.DTOs;

namespace ImageProcessing.Services
{
    public interface IFileStorageService
    {
        Task DeleteFileAsync(string storageKey);

        Task<ImageProcessingResult> SaveImageAsync(IFormFile file);
    }
}