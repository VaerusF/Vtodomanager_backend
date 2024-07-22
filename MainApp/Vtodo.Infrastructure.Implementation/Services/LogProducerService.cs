using System.Text.Json;
using RabbitMQ.Client;
using Vtodo.Entities.Models;
using Vtodo.Infrastructure.Interfaces.Services;

namespace Vtodo.Infrastructure.Implementation.Services;

internal class LogProducerService : ILogProducerService
{
    private readonly IConfigService _configService;

    public LogProducerService(IConfigService configService)
    {
        _configService = configService;
    }
    
    public void SendLog(Log log)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_configService.RabbitMqLoggerConnectionString) };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: $"Logs_{ log.LogLevel.ToString() }",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(log);

        channel.BasicPublish(exchange: "",
            routingKey: $"Logs_{ log.LogLevel.ToString() }",
            basicProperties: null,
            body: body
        );
    }
}