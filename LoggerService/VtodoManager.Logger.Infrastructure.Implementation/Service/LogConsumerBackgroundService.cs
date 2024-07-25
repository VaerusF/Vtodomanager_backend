using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VtodoManager.Logger.Entities.Enums;
using VtodoManager.Logger.Entities.Models;
using VtodoManager.Logger.Infrastructure.Interfaces.Service;

namespace VtodoManager.Logger.Infrastructure.Implementation.Service;

internal class LogConsumerBackgroundService : BackgroundService
{
    private readonly ILogSaverService _logSaverService;

    private IConnection? _connection;
    private IModel? _channelLogDebug;
    private IModel? _channelLogInformation;
    private IModel? _channelLogWarning;
    private IModel? _channelLogError;
    private IModel? _channelLogCritical;
    
    public LogConsumerBackgroundService(IServiceProvider serviceProvider)
    {
        _logSaverService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ILogSaverService>();
        var configService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IConfigService>();
        
        var factory = new ConnectionFactory
        {
            Uri = new Uri(configService.RabbitMqLoggerConnectionString),
            DispatchConsumersAsync = true
        };

        Task.Run(() => InitConnectionAndChannel(factory)).Wait();
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var consumerDebug = new AsyncEventingBasicConsumer(_channelLogDebug);
        consumerDebug.Received += async (sender, args) =>
        {
            var log = JsonSerializer.Deserialize<Log>(args.Body.ToArray());
            
            if (log != null) _logSaverService.SaveLogToDb(log);
            
            _channelLogDebug?.BasicAck(args.DeliveryTag, false);
            await Task.Yield();
        };
        _channelLogDebug?.BasicConsume($"Logs_{CustomLogLevels.Debug.ToString()}", false, consumerDebug);
        
        var consumerInformation = new AsyncEventingBasicConsumer(_channelLogInformation);
        consumerInformation.Received += async (sender, args) =>
        {
            var log = JsonSerializer.Deserialize<Log>(args.Body.ToArray());
            
            if (log != null) _logSaverService.SaveLogToDb(log);
            
            _channelLogInformation?.BasicAck(args.DeliveryTag, false);
            await Task.Yield();
        };
        _channelLogInformation?.BasicConsume($"Logs_{CustomLogLevels.Information.ToString()}", false, consumerInformation);
        
        var consumerWarning = new AsyncEventingBasicConsumer(_channelLogWarning);
        consumerWarning.Received += async (sender, args) =>
        {
            var log = JsonSerializer.Deserialize<Log>(args.Body.ToArray());
            
            if (log != null) _logSaverService.SaveLogToDb(log);
            
            _channelLogWarning?.BasicAck(args.DeliveryTag, false);
            await Task.Yield();
        };
        _channelLogWarning?.BasicConsume($"Logs_{CustomLogLevels.Warning.ToString()}", false, consumerWarning);
        
        var consumerError = new AsyncEventingBasicConsumer(_channelLogError);
        consumerError.Received += async (sender, args) =>
        {
            var log = JsonSerializer.Deserialize<Log>(args.Body.ToArray());
            
            if (log != null) _logSaverService.SaveLogToDb(log);
            
            _channelLogError?.BasicAck(args.DeliveryTag, false);
            await Task.Yield();
        };
        _channelLogError?.BasicConsume($"Logs_{CustomLogLevels.Error.ToString()}", false, consumerError);
        
        var consumerCritical = new AsyncEventingBasicConsumer(_channelLogCritical);
        consumerCritical.Received += async (sender, args) =>
        {
            var log = JsonSerializer.Deserialize<Log>(args.Body.ToArray());
            
            if (log != null) _logSaverService.SaveLogToDb(log);
            
            _channelLogCritical?.BasicAck(args.DeliveryTag, false);
            await Task.Yield();
        };
        _channelLogCritical?.BasicConsume($"Logs_{CustomLogLevels.Critical.ToString()}", false, consumerCritical);
        
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channelLogDebug?.Close();
        _channelLogInformation?.Close();
        _channelLogWarning?.Close();
        _channelLogError?.Close();
        _channelLogCritical?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private async Task InitConnectionAndChannel(IConnectionFactory factory)
    {
        var successfulCreateConnection = false;
        while (!successfulCreateConnection)
        {
            await Task.Delay(5000);
            successfulCreateConnection = TryCreateConnection(factory);
        }
        
        if (_connection != null) _channelLogDebug = _connection.CreateModel();
        if (_connection != null) _channelLogInformation = _connection.CreateModel();
        if (_connection != null) _channelLogWarning = _connection.CreateModel();
        if (_connection != null) _channelLogError = _connection.CreateModel();
        if (_connection != null) _channelLogCritical = _connection.CreateModel();

        _channelLogInformation?.QueueDeclare(queue: $"Logs_{CustomLogLevels.Debug.ToString()}",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        
        _channelLogDebug?.QueueDeclare(queue: $"Logs_{CustomLogLevels.Information.ToString()}",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        
        _channelLogWarning?.QueueDeclare(queue: $"Logs_{CustomLogLevels.Warning.ToString()}",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        
        _channelLogError?.QueueDeclare(queue: $"Logs_{CustomLogLevels.Error.ToString()}",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        
        _channelLogCritical?.QueueDeclare(queue: $"Logs_{CustomLogLevels.Critical.ToString()}",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }
    
    private bool TryCreateConnection(IConnectionFactory factory)
    {
        try
        {
            _connection = factory.CreateConnection();
            return true;
        }
        catch
        {
            return false;
        }
    }

}