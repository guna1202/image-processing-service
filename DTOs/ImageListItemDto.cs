namespace ImageProcessing.DTOs
{
    public class ImageListItemDto
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
}
