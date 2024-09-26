using VtodoManager.NewsService.Entities.Models;

namespace VtodoManager.NewsService.Infrastructure.Interfaces.Services;

internal interface ILogProducerService
{
    void SendLog(Log log);
}