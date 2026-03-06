using ImageProcessing.Data;
using ImageProcessing.DTOs;
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
        private readonly IImageTransformService _imageTransformService;

        public ImagesController(ApplicationDbContext context, IFileStorageService storageService, IConfiguration configuration, IImageTransformService imageTransformService)
        {
            _context = context;
            _storageService = storageService;
            _configuration = configuration;
            _imageTransformService = imageTransformService;
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

            //Delete thumbnail if exists
            if (!string.IsNullOrEmpty(image.ThumbnailStorageKey))
            {
                await _storageService.DeleteFileAsync(image.ThumbnailStorageKey);
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return Ok("Image deleted successfully.");
        }

        [Authorize]
        [HttpGet("{id}/thumbnail")]
        public async Task<IActionResult> GetThumbnail(Guid id)
        {
            var username = User.Identity?.Name;

            var image = await _context.Images
                .FirstOrDefaultAsync(x => x.Id == id);

            if (image == null)
                return NotFound("Image not found.");

            if (image.UploadedBy != username)
                return Forbid();

            if (string.IsNullOrEmpty(image.ThumbnailStorageKey))
                return NotFound("Thumbnail not available.");

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads"
            );

            var filePath = Path.Combine(uploadsFolder, image.ThumbnailStorageKey);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Thumbnail file missing.");

            // Add caching headers (important)
            Response.Headers["Cache-Control"] = "public,max-age=86400";

            return PhysicalFile(
                filePath,
                image.ContentType ?? "image/jpeg",
                enableRangeProcessing: true
            );
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetImages(int page = 1, int pageSize = 10)
        {
            if (page <= 0)
                return BadRequest("Page must be greater than 0.");

            if (pageSize <= 0 || pageSize > 50)
                return BadRequest("PageSize must be between 1 and 50.");

            var username = User.Identity?.Name;

            var query = _context.Images
                .Where(x => x.UploadedBy == username)
                .OrderByDescending(x => x.UploadedAt);

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var images = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ImageListItemDto
                {
                    Id = x.Id,
                    OriginalFileName = x.OriginalFileName,
                    ThumbnailUrl = x.ThumbnailStorageKey != null
                        ? $"/api/images/{x.Id}/thumbnail"
                        : $"/api/images/{x.Id}",
                    UploadedAt = x.UploadedAt
                })
                .ToListAsync();

            var response = new PaginatedResponse<ImageListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = images
            };

            return Ok(response);
        }

        [Authorize]
        [HttpGet("{id}/transform")]
        public async Task<IActionResult> TransformImage(Guid id, [FromQuery] ImageTransformOptions options)
        {
            var username = User.Identity?.Name;

            var image = await _context.Images
                .FirstOrDefaultAsync(x => x.Id == id);

            if (image == null)
                return NotFound("Image not found");

            if (image.UploadedBy != username)
                return Forbid();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            var filePath = Path.Combine(uploadsFolder, image.StoredFileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var stream = await _imageTransformService.TransformAsync(filePath, options);

            var contentType = options.Format?.ToLower()
            switch
            {
                "png" => "image/png",
                "webp" => "image/webp",
                _ => "image/jpeg"
            };

            return File(stream, contentType);
        }

        [HttpGet("capabilities")]
        public IActionResult GetCapabilities()
        {
            return Ok(new
            {
                formats = new[]
                {
                    "jpeg",
                    "png",
                    "webp"
                },
                transformations = new[]
                {
                    "resize",
                    "crop",
                    "rotate",
                    "flip",
                    "grayscale",
                    "compress"
                }
            });
        }
    }
}
