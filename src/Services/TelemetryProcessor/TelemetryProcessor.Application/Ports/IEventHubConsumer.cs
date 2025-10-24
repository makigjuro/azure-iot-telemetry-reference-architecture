using IoTTelemetry.Domain.Entities;

namespace TelemetryProcessor.Application.Ports;

/// <summary>
/// Port for consuming telemetry messages from Event Hubs.
/// </summary>
public interface IEventHubConsumer
{
    /// <summary>
    /// Starts consuming messages from Event Hubs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops consuming messages and gracefully shuts down.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StopAsync(CancellationToken cancellationToken = default);
}
