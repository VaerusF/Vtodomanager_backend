using Microsoft.Extensions.Options;
using VtodoManager.Logger.Infrastructure.Implementation.Options;
using VtodoManager.Logger.Infrastructure.Interfaces.Service;

namespace VtodoManager.Logger.Infrastructure.Implementation.Service;

internal class ConfigService : IConfigService
{
    private readonly ConnectionStringsOptions _connectionStringsOptions;
    
    public ConfigService(
        IOptions<ConnectionStringsOptions> connectionStringsOptions)
    {
        _connectionStringsOptions = connectionStringsOptions.Value;
    }
    
    public string RabbitMqLoggerConnectionString => _connectionStringsOptions.RabbitMqLogger;
}