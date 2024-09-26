namespace VtodoManager.NewsService.Infrastructure.Interfaces.Services
{
    internal interface IConfigService
    {
        int MaxFileSizeInMb { get; }
        string RabbitMqLoggerConnectionString { get; }
    }
}