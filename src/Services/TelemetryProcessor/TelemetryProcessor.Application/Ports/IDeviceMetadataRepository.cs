using IoTTelemetry.Domain.ValueObjects;

namespace TelemetryProcessor.Application.Ports;

/// <summary>
/// Port for retrieving device metadata for enrichment.
/// </summary>
public interface IDeviceMetadataRepository
{
    /// <summary>
    /// Gets metadata for a device.
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Device metadata dictionary or null if not found</returns>
    Task<Dictionary<string, string>?> GetMetadataAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default);
}
