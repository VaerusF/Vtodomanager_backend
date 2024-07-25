using Microsoft.EntityFrameworkCore;
using VtodoManager.Logger.Entities.Models;
using VtodoManager.Logger.Infrastructure.Interfaces.DataAccess;

namespace VtodoManager.Logger.DataAccess.Postgres;

internal class AppDbContext : DbContext, IDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<Log> Logs { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
