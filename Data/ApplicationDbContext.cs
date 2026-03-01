using ImageProcessing.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ImageFile> Images { get; set; }
    }
}
