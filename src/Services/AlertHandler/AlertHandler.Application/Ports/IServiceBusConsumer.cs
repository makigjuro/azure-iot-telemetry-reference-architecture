namespace AlertHandler.Application.Ports;

/// <summary>
/// Port for consuming messages from Azure Service Bus.
/// </summary>
public interface IServiceBusConsumer
{
    /// <summary>
    /// Starts consuming messages from the Service Bus queue.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops consuming messages from the Service Bus queue.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
