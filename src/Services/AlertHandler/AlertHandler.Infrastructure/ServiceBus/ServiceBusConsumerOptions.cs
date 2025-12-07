namespace AlertHandler.Infrastructure.ServiceBus;

/// <summary>
/// Configuration options for Azure Service Bus consumer.
/// </summary>
public sealed class ServiceBusConsumerOptions
{
    public const string SectionName = "ServiceBus";

    /// <summary>
    /// Fully qualified namespace (e.g., sb-iot-dev-mg123.servicebus.windows.net)
    /// </summary>
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    /// <summary>
    /// Name of the Service Bus queue to consume from
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of concurrent calls to message handler
    /// </summary>
    public int MaxConcurrentCalls { get; set; } = 10;

    /// <summary>
    /// Maximum auto lock renewal duration in seconds
    /// </summary>
    public int MaxAutoLockRenewalDurationSeconds { get; set; } = 300;
}
