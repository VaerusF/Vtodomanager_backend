using VtodoManager.Logger.Entities.Models;
using VtodoManager.Logger.Infrastructure.Interfaces.DataAccess;
using VtodoManager.Logger.Infrastructure.Interfaces.Service;

namespace VtodoManager.Logger.Infrastructure.Implementation.Service;

internal class LogSaverService : ILogSaverService
{
    private readonly IDbContext _dbContext;
    
    public LogSaverService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async void SaveLogToDb(Log log)
    {
        log.LogId = new Guid();
        
        _dbContext.Logs.Add(log);
        await _dbContext.SaveChangesAsync();
    }
}