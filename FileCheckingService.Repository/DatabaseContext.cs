using FileCheckingService.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace FileCheckingService.Repository
{
    public class DatabaseContext : DbContext
    {
        public DbSet<FileEntity> Files { get; set; }

        public DatabaseContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
