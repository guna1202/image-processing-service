using ImageProcessing.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ImageProcessing.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public Task DeleteFileAsync(string storageKey)
        {
            var fullPath = Path.Combine(_environment.ContentRootPath, "Uploads", storageKey);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }

        public async Task<ImageProcessingResult> SaveImageAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var storedFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var fullPath = Path.Combine(uploadsFolder, storedFileName);

            // Load image into memory
            using var image = await Image.LoadAsync(file.OpenReadStream());

            // Resize if width > 1920
            if (image.Width > 1920)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(1920, 0)
                }));
            }

            // Save optimized original
            await image.SaveAsync(fullPath, new JpegEncoder
            {
                Quality = 85
            });

            // Generate thumbnail
            var thumbnailFileName = $"thumb_{storedFileName}";
            var thumbnailPath = Path.Combine(uploadsFolder, thumbnailFileName);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(200, 200)
            }));

            await image.SaveAsync(thumbnailPath, new JpegEncoder
            {
                Quality = 75
            });

            return new ImageProcessingResult
            {
                StoredFileName = storedFileName,
                StorageKey = storedFileName,
                Url = $"/uploads/{storedFileName}",
                ThumbnailFileName = thumbnailFileName,
                ThumbnailUrl = $"/uploads/{thumbnailFileName}"
            };
        }
    }
}