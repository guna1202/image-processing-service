using ImageProcessing.Data;
using ImageProcessing.Entities;
using ImageProcessing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessing.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _storageService;

        public ImagesController(ApplicationDbContext context, IFileStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

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
                FileSize = file.Length,
                ContentType = file.ContentType,
                UploadedBy = username
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return Ok(image);
        }
    }
}
