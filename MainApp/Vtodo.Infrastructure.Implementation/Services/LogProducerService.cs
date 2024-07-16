using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Vtodo.Infrastructure.Interfaces.Services;

namespace Vtodo.Infrastructure.Implementation.Services;

internal class LogProducerService : ILogProducerService
{
    private readonly IConfigService _configService;

    public LogProducerService(IConfigService configService)
    {
        _configService = configService;
    }
    
    public void SendLog(LogLevel logLevel, string message)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_configService.RabbitMqLoggerConnectionString) };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: $"Logs_{ logLevel.ToString() }",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
            routingKey: $"Logs_{ logLevel.ToString() }",
            basicProperties: null,
            body: body
        );
    }
}