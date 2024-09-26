using Microsoft.EntityFrameworkCore;
using VtodoManager.NewsService.Entities.Models;

namespace VtodoManager.NewsService.Infrastructure.Interfaces.DataAccess
{
    internal interface IDbContext
    {
        public DbSet<News> News { get; set; }
        public DbSet<NewsFile> NewsFiles { get; set; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        int SaveChanges();
    }
}