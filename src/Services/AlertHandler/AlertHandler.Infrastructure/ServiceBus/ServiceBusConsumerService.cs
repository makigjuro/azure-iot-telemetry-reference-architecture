using System.Text.Json;
using AlertHandler.Application.Commands;
using AlertHandler.Application.Ports;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;

namespace AlertHandler.Infrastructure.ServiceBus;

/// <summary>
/// Service Bus consumer that processes alert messages from Stream Analytics.
/// </summary>
public sealed class ServiceBusConsumerService : IServiceBusConsumer, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<ServiceBusConsumerService> _logger;

    public ServiceBusConsumerService(
        IOptions<ServiceBusConsumerOptions> options,
        IMessageBus messageBus,
        ILogger<ServiceBusConsumerService> logger)
    {
        var config = options.Value;
        _messageBus = messageBus;
        _logger = logger;

        // Create Service Bus client with managed identity
        _client = new ServiceBusClient(
            config.FullyQualifiedNamespace,
            new DefaultAzureCredential());

        // Create processor
        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = config.MaxConcurrentCalls,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(config.MaxAutoLockRenewalDurationSeconds)
        };

        _processor = _client.CreateProcessor(config.QueueName, processorOptions);

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Service Bus consumer");
        await _processor.StartProcessingAsync(cancellationToken);
        _logger.LogInformation("Service Bus consumer started");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping Service Bus consumer");
        await _processor.StopProcessingAsync(cancellationToken);
        _logger.LogInformation("Service Bus consumer stopped");
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            _logger.LogDebug(
                "Received message {MessageId} from Service Bus",
                args.Message.MessageId);

            // Deserialize alert from message body
            var alertData = JsonSerializer.Deserialize<AlertMessage>(args.Message.Body);

            if (alertData is null)
            {
                _logger.LogWarning("Failed to deserialize alert message {MessageId}", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed");
                return;
            }

            // Reconstruct Alert domain entity
            var alert = Alert.Create(
                DeviceId.From(alertData.DeviceId),
                ParseSeverity(alertData.Severity),
                alertData.Message,
                Timestamp.FromDateTimeOffset(alertData.Timestamp),
                alertData.Metadata);

            // Publish to Wolverine message bus
            var command = new ProcessAlertCommand(
                alert,
                args.Message.MessageId,
                args.Message.EnqueuedTime.UtcDateTime);

            await _messageBus.InvokeAsync(command);

            // Complete the message
            await args.CompleteMessageAsync(args.Message);

            _logger.LogInformation(
                "Successfully processed alert {AlertId} from message {MessageId}",
                alert.Id,
                args.Message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing message {MessageId}",
                args.Message.MessageId);

            // Message will be retried or moved to dead-letter queue
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus error. Source: {ErrorSource}, Entity: {EntityPath}",
            args.ErrorSource,
            args.EntityPath);

        return Task.CompletedTask;
    }

    private static AlertSeverity ParseSeverity(string severity)
    {
        return severity.ToLowerInvariant() switch
        {
            "info" => AlertSeverity.Info,
            "warning" => AlertSeverity.Warning,
            "error" => AlertSeverity.Error,
            "critical" => AlertSeverity.Critical,
            _ => AlertSeverity.Warning
        };
    }

    public async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
        await _client.DisposeAsync();
    }

    /// <summary>
    /// DTO for alert messages from Service Bus.
    /// </summary>
    private sealed record AlertMessage(
        string DeviceId,
        string Severity,
        string Message,
        DateTimeOffset Timestamp,
        Dictionary<string, object>? Metadata);
}
