namespace Vtodo.Infrastructure.Interfaces.Services
{
    internal interface IConfigService
    {
        int HasherIterations { get; }
        int HasherKeySize { get; }
        int MaxProjectFileSizeInMb { get; }
        string RabbitMqLoggerConnectionString { get; }
        string FrontClientAddress { get; }
    }
}