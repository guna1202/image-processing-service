using ImageProcessing.Data;
using ImageProcessing.Entities;
using ImageProcessing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessing.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _storageService;
        private readonly IConfiguration _configuration;

        public ImagesController(ApplicationDbContext context, IFileStorageService storageService, IConfiguration configuration)
        {
            _context = context;
            _storageService = storageService;
            _configuration = configuration;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            var maxFileSizeMB = _configuration.GetValue<int>("FileUpload:MaxFileSizeMB");
            var maxBytes = maxFileSizeMB * 1024 * 1024;

            if (file.Length > maxBytes)
                return BadRequest($"File size exceeds {maxFileSizeMB} MB limit.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Unsupported file type.");

            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedContentTypes.Contains(file.ContentType))
                return BadRequest("Invalid content type.");

            var result = await _storageService.SaveImageAsync(file);

            var username = User.Identity?.Name ?? "Unknown";

            var image = new ImageFile
            {
                Id = Guid.NewGuid(),
                OriginalFileName = file.FileName,
                StoredFileName = result.StoredFileName,
                StorageProvider = "Local",
                StorageKey = result.StorageKey,
                Url = result.Url,

                ThumbnailFileName = result.ThumbnailFileName,
                ThumbnailStorageKey = result.ThumbnailFileName,
                ThumbnailUrl = result.ThumbnailUrl,

                FileSize = file.Length,
                ContentType = file.ContentType,
                UploadedBy = username
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return Ok(image);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var username = User.Identity?.Name;

            var image = await _context.Images
                .FirstOrDefaultAsync(x => x.Id == id);

            if (image == null)
                return NotFound("Image not found.");

            // Owner validation (important)
            if (image.UploadedBy != username)
                return Forbid();

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads"
            );

            var filePath = Path.Combine(uploadsFolder, image.StoredFileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on disk.");

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return File(fileStream, image.ContentType);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            var username = User.Identity?.Name;

            var image = await _context.Images
                .FirstOrDefaultAsync(x => x.Id == id);

            if (image == null)
                return NotFound("Image not found.");

            if (image.UploadedBy != username)
                return Forbid();

            await _storageService.DeleteFileAsync(image.StorageKey);

            // If you stored thumbnail, delete it too
            //if (!string.IsNullOrEmpty(image.ThumbnailStorageKey))
            //{
            //    await _storageService.DeleteFileAsync(image.ThumbnailStorageKey);
            //}

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return Ok("Image deleted successfully.");
        }
    }
}
