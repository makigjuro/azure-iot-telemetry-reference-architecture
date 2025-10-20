using System.Diagnostics.Metrics;

namespace IoTTelemetry.Shared.Infrastructure.Observability;

/// <summary>
/// Provides pre-configured metric instruments for the application.
/// This is a singleton pattern to ensure consistent metric collection across all services.
/// </summary>
public static class InstrumentationMetrics
{
    private static readonly Meter _meter = new(
        TelemetryConstants.MeterName,
        version: typeof(InstrumentationMetrics).Assembly.GetName().Version?.ToString() ?? "1.0.0");

    /// <summary>
    /// Gets the global Meter for the application.
    /// </summary>
    public static Meter Meter => _meter;

    // ========== Message Counters ==========

    /// <summary>
    /// Counter for total messages received from all sources.
    /// Tags: messaging.system, messaging.destination.name, messaging.consumer.group.name
    /// </summary>
    public static readonly Counter<long> MessagesReceived = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.MessagesReceived,
        unit: "{message}",
        description: "Total number of messages received from messaging systems");

    /// <summary>
    /// Counter for successfully processed messages.
    /// Tags: messaging.system, processing.stage
    /// </summary>
    public static readonly Counter<long> MessagesProcessed = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.MessagesProcessed,
        unit: "{message}",
        description: "Total number of messages processed successfully");

    /// <summary>
    /// Counter for failed message processing.
    /// Tags: messaging.system, error.type
    /// </summary>
    public static readonly Counter<long> MessagesFailed = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.MessagesFailed,
        unit: "{message}",
        description: "Total number of messages that failed processing");

    // ========== Duration Histograms ==========

    /// <summary>
    /// Histogram for operation duration (e.g., Azure SDK calls).
    /// Tags: messaging.system, messaging.operation
    /// </summary>
    public static readonly Histogram<double> OperationDuration = _meter.CreateHistogram<double>(
        TelemetryConstants.Metrics.OperationDuration,
        unit: "ms",
        description: "Duration of operations in milliseconds");

    /// <summary>
    /// Histogram for message processing duration.
    /// Tags: processing.stage
    /// </summary>
    public static readonly Histogram<double> ProcessingDuration = _meter.CreateHistogram<double>(
        TelemetryConstants.Metrics.ProcessingDuration,
        unit: "ms",
        description: "Duration of message processing in milliseconds");

    // ========== Event Hubs Specific ==========

    /// <summary>
    /// Counter for Event Hubs messages.
    /// Tags: messaging.destination.name, messaging.operation
    /// </summary>
    public static readonly Counter<long> EventHubsMessages = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.EventHubsMessageCount,
        unit: "{message}",
        description: "Number of Event Hubs messages sent/received");

    /// <summary>
    /// Histogram for Event Hubs batch sizes.
    /// Tags: messaging.destination.name
    /// </summary>
    public static readonly Histogram<int> EventHubsBatchSize = _meter.CreateHistogram<int>(
        TelemetryConstants.Metrics.EventHubsBatchSize,
        unit: "{message}",
        description: "Size of Event Hubs message batches");

    // ========== IoT Hub Specific ==========

    /// <summary>
    /// Counter for IoT Hub C2D commands sent.
    /// Tags: iot.device.id
    /// </summary>
    public static readonly Counter<long> IoTHubCommandsSent = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.IoTHubCommandsSent,
        unit: "{command}",
        description: "Number of IoT Hub cloud-to-device commands sent");

    // ========== Storage Specific ==========

    /// <summary>
    /// Counter for bytes written to Azure Storage.
    /// Tags: azure.storage.container, processing.stage
    /// </summary>
    public static readonly Counter<long> StorageBytesWritten = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.StorageBytesWritten,
        unit: "By",
        description: "Total bytes written to Azure Storage");

    /// <summary>
    /// Counter for bytes read from Azure Storage.
    /// Tags: azure.storage.container
    /// </summary>
    public static readonly Counter<long> StorageBytesRead = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.StorageBytesRead,
        unit: "By",
        description: "Total bytes read from Azure Storage");

    // ========== Database Specific ==========

    /// <summary>
    /// Counter for database queries executed.
    /// Tags: db.system, db.operation
    /// </summary>
    public static readonly Counter<long> DatabaseQueriesExecuted = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.DatabaseQueriesExecuted,
        unit: "{query}",
        description: "Number of database queries executed");

    // ========== Business Metrics ==========

    /// <summary>
    /// Observable gauge for active devices.
    /// This should be updated by the application periodically.
    /// </summary>
    public static ObservableGauge<int> CreateActiveDevicesGauge(Func<int> observeValue)
    {
        return _meter.CreateObservableGauge(
            TelemetryConstants.Metrics.DevicesActive,
            observeValue,
            unit: "{device}",
            description: "Number of active devices");
    }

    /// <summary>
    /// Counter for alerts triggered.
    /// Tags: alert.severity, iot.device.id
    /// </summary>
    public static readonly Counter<long> AlertsTriggered = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.AlertsTriggered,
        unit: "{alert}",
        description: "Number of alerts triggered");

    /// <summary>
    /// Counter for telemetry readings ingested.
    /// Tags: iot.telemetry.type, processing.stage
    /// </summary>
    public static readonly Counter<long> TelemetryReadingsIngested = _meter.CreateCounter<long>(
        TelemetryConstants.Metrics.TelemetryReadingsIngested,
        unit: "{reading}",
        description: "Number of telemetry readings ingested");
}
