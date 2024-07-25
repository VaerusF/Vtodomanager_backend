using VtodoManager.Logger.Entities.Models;

namespace VtodoManager.Logger.Infrastructure.Interfaces.Service;

internal interface ILogSaverService
{
    void SaveLogToDb(Log log);
}