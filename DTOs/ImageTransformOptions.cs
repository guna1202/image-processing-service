namespace ImageProcessing.DTOs
{
    public class ImageTransformOptions
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Rotate { get; set; }
        public bool Grayscale { get; set; }
        public bool Flip { get; set; }
        public int? Quality { get; set; }
        public string? Format { get; set; }
    }
}
