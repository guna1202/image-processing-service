using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ImageProcessing.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(4)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
