using Microsoft.EntityFrameworkCore;
using VtodoManager.Logger.Entities.Models;

namespace VtodoManager.Logger.Infrastructure.Interfaces.DataAccess;

internal interface IDbContext
{
    public DbSet<Log> Logs { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    int SaveChanges();
}
