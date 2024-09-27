using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VtodoManager.NewsService.DataAccess.Postgres;

namespace VtodoManager.NewsService.Tests.Utils
{
    public static class TestDbUtils
    {
        internal static AppDbContext SetupTestDbContextInMemory()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
        
        internal static IDistributedCache SetupTestCacheInMemory()
        {
            var options = Options.Create(new MemoryDistributedCacheOptions());
            
            return new MemoryDistributedCache(options);
        }
    }
}