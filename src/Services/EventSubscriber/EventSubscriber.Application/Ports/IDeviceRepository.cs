using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;

namespace EventSubscriber.Application.Ports;

/// <summary>
/// Repository for device metadata persistence.
/// </summary>
public interface IDeviceRepository
{
    /// <summary>
    /// Saves or updates a device in the repository.
    /// </summary>
    Task SaveDeviceAsync(Device device, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a device by its ID.
    /// </summary>
    Task<Device?> GetDeviceByIdAsync(DeviceId deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a device from the repository.
    /// </summary>
    Task DeleteDeviceAsync(DeviceId deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a device exists.
    /// </summary>
    Task<bool> DeviceExistsAsync(DeviceId deviceId, CancellationToken cancellationToken = default);
}
