using Microsoft.EntityFrameworkCore;
using VtodoManager.NewsService.Entities.Models;
using VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess;

namespace VtodoManager.NewsService.DataAccess.Postgres
{
    internal class AppDbContext : DbContext, IDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public DbSet<News> News { get; set; } = null!;
        public DbSet<NewsFile> NewsFiles { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsFile>()
                .HasKey(m => new { m.NewsId, m.FileName });

            modelBuilder.Entity<News>()
                .HasIndex(e => new { e.Title, e.Content });
        }
    }
}