namespace ImageProcessing.DTOs
{
    public class ImageProcessingResult
    {
        public string StoredFileName { get; set; } = null!;
        public string StorageKey { get; set; } = null!;
        public string Url { get; set; } = null!;

        public string ThumbnailFileName { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
    }
}
