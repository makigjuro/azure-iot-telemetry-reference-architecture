namespace IoTTelemetry.Shared.Infrastructure.Observability;

/// <summary>
/// Constants for OpenTelemetry instrumentation (activity names, meter names, and semantic tag keys).
/// Follows OpenTelemetry semantic conventions for Azure and messaging systems.
/// </summary>
public static class TelemetryConstants
{
    /// <summary>
    /// Base name for the ActivitySource used across all services.
    /// </summary>
    public const string ActivitySourceName = "IoTTelemetry";

    /// <summary>
    /// Base name for the Meter used across all services.
    /// </summary>
    public const string MeterName = "IoTTelemetry";

    /// <summary>
    /// Activity names for distributed tracing.
    /// </summary>
    public static class Activities
    {
        // Event Hubs operations
        public const string EventHubsReceive = "eventhubs.receive";
        public const string EventHubsSend = "eventhubs.send";
        public const string EventHubsProcess = "eventhubs.process";

        // IoT Hub operations
        public const string IoTHubReceiveC2D = "iothub.receive_c2d";
        public const string IoTHubSendC2D = "iothub.send_c2d";
        public const string IoTHubTelemetry = "iothub.telemetry";

        // Storage operations
        public const string StorageUpload = "storage.upload";
        public const string StorageDownload = "storage.download";
        public const string StorageList = "storage.list";

        // Digital Twins operations
        public const string DigitalTwinsUpdate = "digitaltwin.update";
        public const string DigitalTwinsQuery = "digitaltwin.query";

        // Database operations
        public const string DatabaseQuery = "database.query";
        public const string DatabaseCommand = "database.command";

        // Service Bus operations
        public const string ServiceBusReceive = "servicebus.receive";
        public const string ServiceBusSend = "servicebus.send";

        // Domain events
        public const string DomainEventPublish = "domain_event.publish";
        public const string DomainEventHandle = "domain_event.handle";

        // Telemetry processing
        public const string TelemetryValidate = "telemetry.validate";
        public const string TelemetryTransform = "telemetry.transform";
        public const string TelemetryAggregate = "telemetry.aggregate";
    }

    /// <summary>
    /// Metric instrument names.
    /// </summary>
    public static class Metrics
    {
        // Message counters
        public const string MessagesReceived = "messages.received";
        public const string MessagesProcessed = "messages.processed";
        public const string MessagesFailed = "messages.failed";

        // Operation durations
        public const string OperationDuration = "operation.duration";
        public const string ProcessingDuration = "processing.duration";

        // Azure service specific
        public const string EventHubsMessageCount = "eventhubs.messages";
        public const string EventHubsBatchSize = "eventhubs.batch_size";
        public const string IoTHubCommandsSent = "iothub.commands_sent";
        public const string StorageBytesWritten = "storage.bytes_written";
        public const string StorageBytesRead = "storage.bytes_read";
        public const string DatabaseQueriesExecuted = "database.queries_executed";

        // Business metrics
        public const string DevicesActive = "devices.active";
        public const string AlertsTriggered = "alerts.triggered";
        public const string TelemetryReadingsIngested = "telemetry.readings_ingested";
    }

    /// <summary>
    /// Semantic tag keys following OpenTelemetry conventions.
    /// </summary>
    public static class Tags
    {
        // Service attributes
        public const string ServiceName = "service.name";
        public const string ServiceVersion = "service.version";
        public const string DeploymentEnvironment = "deployment.environment";

        // Azure resource attributes
        public const string AzureResourceGroup = "azure.resource_group";
        public const string AzureSubscription = "azure.subscription_id";

        // Messaging attributes (OpenTelemetry semantic conventions)
        public const string MessagingSystem = "messaging.system";
        public const string MessagingDestination = "messaging.destination.name";
        public const string MessagingOperation = "messaging.operation";
        public const string MessagingMessageId = "messaging.message.id";
        public const string MessagingBatchSize = "messaging.batch.message_count";
        public const string MessagingConsumerGroup = "messaging.consumer.group.name";

        // IoT specific attributes
        public const string DeviceId = "iot.device.id";
        public const string DeviceType = "iot.device.type";
        public const string DeviceStatus = "iot.device.status";
        public const string TelemetryType = "iot.telemetry.type";

        // Storage attributes
        public const string StorageContainer = "azure.storage.container";
        public const string StorageBlobName = "azure.storage.blob.name";
        public const string StorageBytesTransferred = "azure.storage.bytes_transferred";

        // Database attributes
        public const string DatabaseSystem = "db.system";
        public const string DatabaseName = "db.name";
        public const string DatabaseOperation = "db.operation";
        public const string DatabaseStatement = "db.statement";

        // Error attributes
        public const string ErrorType = "error.type";
        public const string ErrorMessage = "error.message";
        public const string ExceptionStackTrace = "exception.stacktrace";

        // Custom attributes
        public const string CorrelationId = "correlation.id";
        public const string ProcessingStage = "processing.stage";
        public const string RetryAttempt = "retry.attempt";
    }

    /// <summary>
    /// Tag values for messaging.system attribute.
    /// </summary>
    public static class MessagingSystems
    {
        public const string EventHubs = "azure_eventhubs";
        public const string ServiceBus = "azure_servicebus";
        public const string IoTHub = "azure_iothub";
    }

    /// <summary>
    /// Tag values for messaging.operation attribute.
    /// </summary>
    public static class MessagingOperations
    {
        public const string Receive = "receive";
        public const string Send = "send";
        public const string Process = "process";
        public const string Publish = "publish";
    }

    /// <summary>
    /// Tag values for processing.stage attribute.
    /// </summary>
    public static class ProcessingStages
    {
        public const string Raw = "raw";
        public const string Bronze = "bronze";
        public const string Silver = "silver";
        public const string Gold = "gold";
        public const string Validation = "validation";
        public const string Transformation = "transformation";
        public const string Aggregation = "aggregation";
    }
}
