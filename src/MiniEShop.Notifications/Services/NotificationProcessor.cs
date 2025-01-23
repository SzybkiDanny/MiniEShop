namespace MiniEShop.Notifications.Services;

using System.Text.Json;
using Azure.Messaging.ServiceBus;
using MiniEShop.Notifications.Models;
using Microsoft.Extensions.Options;

public class NotificationProcessor : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<NotificationProcessor> _logger;

    public NotificationProcessor(
        IOptions<ServiceBusSettings> settings,
        ILogger<NotificationProcessor> logger)
    {
        _client = new ServiceBusClient(settings.Value.ConnectionString);
        _processor = _client.CreateProcessor(settings.Value.QueueName);
        _logger = logger;

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _processor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("Started processing messages from Service Bus queue: {QueueName}", 
                _processor.EntityPath);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error processing messages from Service Bus queue");
            throw;
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var notification = JsonSerializer.Deserialize<OrderNotification>(args.Message.Body);
            if (notification == null)
            {
                _logger.LogWarning("Received null notification from message: {MessageId}", 
                    args.Message.MessageId);
                return;
            }

            _logger.LogInformation(
                "Would send notification for order {OrderId} to user {UserId}. Total amount: {TotalAmount:C}",
                notification.OrderId,
                notification.UserId,
                notification.TotalAmount);

            // Complete the message to remove it from the queue
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {MessageId}", args.Message.MessageId);
            throw;
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error in message processing");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping notification processor");
            throw;
        }
    }
}

public class ServiceBusSettings
{
    public string ConnectionString { get; set; } = default!;
    public string QueueName { get; set; } = default!;
}
