using Microsoft.Extensions.Options;
using Vtodo.Infrastructure.Implementation.Options;
using Vtodo.Infrastructure.Interfaces.Services;

namespace Vtodo.Infrastructure.Implementation.Services
{
    internal class ConfigService : IConfigService
    {
        private readonly HasherOptions _hasherOptions;
        private readonly ProjectFilesOptions _projectFilesOptions;
        private readonly IpListOptions _ipListOptions;
        private readonly ConnectionStringsOptions _connectionStringsOptions;
        
        public ConfigService(
            IOptions<HasherOptions> hasherOptions, 
            IOptions<ProjectFilesOptions> projectFilesOptions, 
            IOptions<IpListOptions> ipListOptions,
            IOptions<ConnectionStringsOptions> connectionStringsOptions)
        {
            _hasherOptions = hasherOptions.Value;
            _projectFilesOptions = projectFilesOptions.Value;
            _ipListOptions = ipListOptions.Value;
            _connectionStringsOptions = connectionStringsOptions.Value;
        }
        
        public int HasherIterations => _hasherOptions.Iterations;

        public int HasherKeySize => _hasherOptions.KeySize;

        public int MaxProjectFileSizeInMb => _projectFilesOptions.MaxProjectFileSizeInMb;
        public string FrontClientAddress => _ipListOptions.FrontClientAddress;
        public string RabbitMqLoggerConnectionString => _connectionStringsOptions.RabbitMqLogger;
    }
}