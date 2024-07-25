namespace VtodoManager.Logger.Infrastructure.Interfaces.Service;

internal interface IConfigService
{
    string RabbitMqLoggerConnectionString { get; }
}