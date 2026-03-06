using System.ComponentModel.DataAnnotations;

namespace ImageProcessing.DTOs
{
    public class ImageTransformOptions
    {
        [Range(1, 4000)]
        public int? Width { get; set; }
        [Range(1, 4000)]
        public int? Height { get; set; }
        public int? Rotate { get; set; }
        public bool Grayscale { get; set; }
        public bool Flip { get; set; }
        [Range(1, 100)]
        public int? Quality { get; set; }
        public string? Format { get; set; }
    }
}
