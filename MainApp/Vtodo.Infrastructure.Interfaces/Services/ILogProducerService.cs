using Vtodo.Entities.Models;

namespace Vtodo.Infrastructure.Interfaces.Services;

internal interface ILogProducerService
{
    void SendLog(Log log);
}