using Microsoft.Extensions.Logging;

namespace Vtodo.Infrastructure.Interfaces.Services;

internal interface ILogProducerService
{
    void SendLog(LogLevel logLevel, string message);
}