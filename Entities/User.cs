using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ImageProcessing.Entities
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
