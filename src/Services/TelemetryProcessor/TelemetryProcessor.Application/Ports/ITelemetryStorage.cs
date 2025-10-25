using IoTTelemetry.Domain.Entities;

namespace TelemetryProcessor.Application.Ports;

/// <summary>
/// Port for persisting telemetry data to the data lake.
/// </summary>
public interface ITelemetryStorage
{
    /// <summary>
    /// Stores raw telemetry in the bronze layer.
    /// </summary>
    /// <param name="reading">Telemetry reading</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StoreBronzeAsync(TelemetryReading reading, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores validated and enriched telemetry in the silver layer.
    /// </summary>
    /// <param name="reading">Telemetry reading</param>
    /// <param name="enrichedMetadata">Additional metadata from device lookup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StoreSilverAsync(
        TelemetryReading reading,
        Dictionary<string, string> enrichedMetadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores aggregated metrics in the gold layer.
    /// </summary>
    /// <param name="aggregates">Hourly aggregates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StoreGoldAsync(
        Dictionary<string, object> aggregates,
        CancellationToken cancellationToken = default);
}
