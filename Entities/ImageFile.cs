using System.ComponentModel.DataAnnotations;

namespace ImageProcessing.Entities
{
    public class ImageFile
    {
        public Guid Id { get; set; }

        [Required]
        public string OriginalFileName { get; set; } = null!;

        [Required]
        public string StoredFileName { get; set; } = null!;

        [Required]
        public string StorageProvider { get; set; } = null!; // Local, Azure, etc.

        [Required]
        public string StorageKey { get; set; } = null!; // Path or blob key

        public string? Url { get; set; }

        public string? ThumbnailFileName { get; set; }

        public string? ThumbnailStorageKey { get; set; }

        public string? ThumbnailUrl { get; set; }

        public long FileSize { get; set; }

        public string? ContentType { get; set; }

        public string UploadedBy { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}