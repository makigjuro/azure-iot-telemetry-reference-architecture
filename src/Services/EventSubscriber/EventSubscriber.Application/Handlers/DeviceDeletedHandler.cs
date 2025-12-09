using EventSubscriber.Application.Commands;
using EventSubscriber.Application.Ports;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EventSubscriber.Application.Handlers;

/// <summary>
/// Handles device deletion events from Event Grid.
/// Marks device as decommissioned in PostgreSQL and cascades to Digital Twin deletion.
/// </summary>
public sealed class DeviceDeletedHandler
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<DeviceDeletedHandler> _logger;

    public DeviceDeletedHandler(
        IDeviceRepository deviceRepository,
        ILogger<DeviceDeletedHandler> logger)
    {
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    public async Task<SyncDigitalTwinCommand?> Handle(
        DeviceDeletedCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing device deleted event for device {DeviceId}",
                command.DeviceId);

            var deviceId = DeviceId.Create(command.DeviceId);

            // Retrieve the device
            var device = await _deviceRepository.GetDeviceByIdAsync(deviceId, cancellationToken);
            if (device is null)
            {
                _logger.LogWarning(
                    "Device {DeviceId} not found. May have already been deleted.",
                    command.DeviceId);

                // Still cascade to Digital Twin deletion to ensure cleanup
                return new SyncDigitalTwinCommand(
                    deviceId,
                    SyncOperation.Delete);
            }

            // Mark as decommissioned (soft delete)
            device.Decommission();

            // Save the updated state
            await _deviceRepository.SaveDeviceAsync(device, cancellationToken);

            _logger.LogInformation(
                "Device {DeviceId} marked as decommissioned",
                command.DeviceId);

            // Cascade to Digital Twin deletion
            return new SyncDigitalTwinCommand(
                deviceId,
                SyncOperation.Delete);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process device deleted event for device {DeviceId}",
                command.DeviceId);
            throw;
        }
    }
}
