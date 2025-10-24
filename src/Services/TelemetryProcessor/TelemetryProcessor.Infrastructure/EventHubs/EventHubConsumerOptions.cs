namespace TelemetryProcessor.Infrastructure.EventHubs;

/// <summary>
/// Configuration options for Event Hubs consumer.
/// </summary>
public sealed class EventHubConsumerOptions
{
    public const string SectionName = "EventHub";

    /// <summary>
    /// Event Hubs namespace (e.g., "eh-iot-dev-mg123.servicebus.windows.net")
    /// </summary>
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    /// <summary>
    /// Event Hub name (e.g., "telemetry")
    /// </summary>
    public string EventHubName { get; set; } = string.Empty;

    /// <summary>
    /// Consumer group name (default: "$Default")
    /// </summary>
    public string ConsumerGroup { get; set; } = "$Default";

    /// <summary>
    /// Blob container name for checkpointing (e.g., "checkpoints")
    /// </summary>
    public string CheckpointBlobContainer { get; set; } = string.Empty;

    /// <summary>
    /// Storage account name for checkpoints (e.g., "stiotdevmg123")
    /// </summary>
    public string CheckpointStorageAccount { get; set; } = string.Empty;

    /// <summary>
    /// Maximum batch size for processing (default: 100)
    /// </summary>
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum wait time for batch in seconds (default: 10)
    /// </summary>
    public int MaxWaitTimeSeconds { get; set; } = 10;
}
