using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Ports;
using Wolverine;

namespace TelemetryProcessor.Infrastructure.EventHubs;

/// <summary>
/// Event Hubs consumer service that processes telemetry messages.
/// Implements IEventHubConsumer and uses EventProcessorClient for checkpointing.
/// </summary>
public sealed class EventHubConsumerService : BackgroundService, IEventHubConsumer
{
    private readonly EventHubConsumerOptions _options;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<EventHubConsumerService> _logger;
    private EventProcessorClient? _processor;

    public EventHubConsumerService(
        IOptions<EventHubConsumerOptions> options,
        IMessageBus messageBus,
        ILogger<EventHubConsumerService> logger)
    {
        _options = options.Value;
        _messageBus = messageBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartAsync(stoppingToken);

        // Keep running until cancellation
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Expected when stopping
        }

        await StopAsync(stoppingToken);
    }

    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting Event Hubs consumer for {Namespace}/{EventHub} with consumer group {ConsumerGroup}",
            _options.FullyQualifiedNamespace,
            _options.EventHubName,
            _options.ConsumerGroup);

        // Create blob container client for checkpointing (uses managed identity)
        var blobContainerUri = new Uri(
            $"https://{_options.CheckpointStorageAccount}.blob.core.windows.net/{_options.CheckpointBlobContainer}");

        var blobContainerClient = new BlobContainerClient(
            blobContainerUri,
            new DefaultAzureCredential());

        // Ensure checkpoint container exists
        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        // Create Event Processor Client (uses managed identity)
        _processor = new EventProcessorClient(
            blobContainerClient,
            _options.ConsumerGroup,
            _options.FullyQualifiedNamespace,
            _options.EventHubName,
            new DefaultAzureCredential());

        // Register event handlers
        _processor.ProcessEventAsync += ProcessEventHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;

        // Start processing
        await _processor.StartProcessingAsync(cancellationToken);

        _logger.LogInformation("Event Hubs consumer started successfully");
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_processor == null)
        {
            return;
        }

        _logger.LogInformation("Stopping Event Hubs consumer...");

        // Unregister handlers
        _processor.ProcessEventAsync -= ProcessEventHandler;
        _processor.ProcessErrorAsync -= ProcessErrorHandler;

        // Stop processing (drains in-flight messages)
        await _processor.StopProcessingAsync(cancellationToken);

        _logger.LogInformation("Event Hubs consumer stopped successfully");
    }

    private async Task ProcessEventHandler(ProcessEventArgs args)
    {
        try
        {
            if (args.Data == null || args.Data.EventBody.ToArray().Length == 0)
            {
                _logger.LogWarning(
                    "Received empty event from partition {PartitionId}",
                    args.Partition.PartitionId);
                return;
            }

            _logger.LogDebug(
                "Processing event from partition {PartitionId}, sequence {SequenceNumber}",
                args.Partition.PartitionId,
                args.Data!.SequenceNumber);

            // Deserialize telemetry
            var telemetryReading = DeserializeTelemetry(args.Data);

            if (telemetryReading == null)
            {
                _logger.LogWarning(
                    "Failed to deserialize telemetry from partition {PartitionId}, sequence {SequenceNumber}",
                    args.Partition.PartitionId,
                    args.Data.SequenceNumber);

                // Checkpoint failed message to avoid reprocessing
                await args.UpdateCheckpointAsync(args.CancellationToken);
                return;
            }

            // Publish to Wolverine message bus
            var command = new ProcessTelemetryCommand(
                telemetryReading,
                args.Partition.PartitionId,
                args.Data.SequenceNumber);

            await _messageBus.InvokeAsync(command, args.CancellationToken);

            // Checkpoint after successful processing
            await args.UpdateCheckpointAsync(args.CancellationToken);

            _logger.LogDebug(
                "Successfully processed and checkpointed event from partition {PartitionId}, sequence {SequenceNumber}",
                args.Partition.PartitionId,
                args.Data.SequenceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing event from partition {PartitionId}, sequence {SequenceNumber}",
                args.Partition.PartitionId,
                args.Data.SequenceNumber);

            // Don't checkpoint on error - message will be reprocessed
            // Consider implementing dead-letter queue for persistent failures
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Error in Event Hubs processor on partition {PartitionId}, operation {Operation}",
            args.PartitionId ?? "unknown",
            args.Operation);

        return Task.CompletedTask;
    }

    private TelemetryReading? DeserializeTelemetry(EventData eventData)
    {
        try
        {
            var json = Encoding.UTF8.GetString(eventData.EventBody.ToArray());
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Extract device ID
            var deviceIdStr = root.GetProperty("deviceId").GetString();
            if (string.IsNullOrWhiteSpace(deviceIdStr))
            {
                return null;
            }

            var deviceId = DeviceId.Create(deviceIdStr);

            // Extract timestamp
            var timestamp = root.TryGetProperty("timestamp", out var tsElement)
                ? DateTimeOffset.Parse(tsElement.GetString()!)
                : eventData.EnqueuedTime;

            // Extract measurements
            var measurements = new Dictionary<string, TelemetryValue>();

            if (root.TryGetProperty("measurements", out var measurementsElement))
            {
                foreach (var prop in measurementsElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number)
                    {
                        var value = prop.Value.GetDouble();
                        var unit = root.TryGetProperty($"{prop.Name}_unit", out var unitElement)
                            ? unitElement.GetString() ?? "unknown"
                            : "unknown";

                        measurements[prop.Name] = TelemetryValue.Create(value, unit);
                    }
                }
            }
            else
            {
                // Fallback: treat all numeric properties as measurements
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number &&
                        prop.Name != "timestamp" &&
                        prop.Name != "deviceId")
                    {
                        var value = prop.Value.GetDouble();
                        measurements[prop.Name] = TelemetryValue.Create(value, "unknown");
                    }
                }
            }

            if (measurements.Count == 0)
            {
                _logger.LogWarning("No measurements found in telemetry for device {DeviceId}", deviceId);
                return null;
            }

            return TelemetryReading.Create(
                deviceId,
                Timestamp.Create(timestamp),
                measurements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing telemetry");
            return null;
        }
    }

    public override void Dispose()
    {
        if (_processor != null)
        {
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
        base.Dispose();
    }
}
