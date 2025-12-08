using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;

namespace EventSubscriber.Application.Ports;

/// <summary>
/// Service for synchronizing devices with Azure Digital Twins.
/// </summary>
public interface IDigitalTwinService
{
    /// <summary>
    /// Creates or updates a digital twin for the specified device.
    /// </summary>
    Task<bool> CreateOrUpdateTwinAsync(Device device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a digital twin for the specified device.
    /// </summary>
    Task<bool> DeleteTwinAsync(DeviceId deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a digital twin exists.
    /// </summary>
    Task<bool> TwinExistsAsync(DeviceId deviceId, CancellationToken cancellationToken = default);
}
