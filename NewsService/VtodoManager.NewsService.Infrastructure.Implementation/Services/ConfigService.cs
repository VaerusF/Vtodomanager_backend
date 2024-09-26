using Microsoft.Extensions.Options;
using VtodoManager.NewsService.Infrastructure.Implementation.Options;
using VtodoManager.NewsService.Infrastructure.Interfaces.Services;

namespace VtodoManager.NewsService.Infrastructure.Implementation.Services
{
    internal class ConfigService : IConfigService
    {
        private readonly ProjectFilesOptions _projectFilesOptions;
        private readonly ConnectionStringsOptions _connectionStringsOptions;
        
        public ConfigService(
            IOptions<ProjectFilesOptions> projectFilesOptions, 
            IOptions<ConnectionStringsOptions> connectionStringsOptions)
        {
            _projectFilesOptions = projectFilesOptions.Value;
            _connectionStringsOptions = connectionStringsOptions.Value;
        }
        
        public int MaxFileSizeInMb => _projectFilesOptions.MaxFileSizeInMb;
        public string RabbitMqLoggerConnectionString => _connectionStringsOptions.RabbitMqLogger;
    }
}