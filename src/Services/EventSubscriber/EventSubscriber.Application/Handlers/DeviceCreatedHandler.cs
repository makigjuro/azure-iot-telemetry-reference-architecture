using EventSubscriber.Application.Commands;
using EventSubscriber.Application.Ports;
using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace EventSubscriber.Application.Handlers;

/// <summary>
/// Handles device creation events from Event Grid.
/// Creates device entity in PostgreSQL and cascades to Digital Twin sync.
/// </summary>
public sealed class DeviceCreatedHandler
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<DeviceCreatedHandler> _logger;

    public DeviceCreatedHandler(
        IDeviceRepository deviceRepository,
        ILogger<DeviceCreatedHandler> logger)
    {
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    public async Task<SyncDigitalTwinCommand?> Handle(
        DeviceCreatedCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing device created event for device {DeviceId}",
                command.DeviceId);

            var deviceId = DeviceId.Create(command.DeviceId);

            // Check if device already exists (idempotency)
            var existingDevice = await _deviceRepository.GetDeviceByIdAsync(deviceId, cancellationToken);
            if (existingDevice is not null)
            {
                _logger.LogWarning(
                    "Device {DeviceId} already exists. Skipping duplicate creation.",
                    command.DeviceId);
                return null;
            }

            // Extract device metadata from Event Grid data
            var deviceName = command.Data?.TryGetValue("deviceName", out var name) == true
                ? name.ToString() ?? command.DeviceId
                : command.DeviceId;

            var deviceType = command.Data?.TryGetValue("deviceType", out var type) == true
                ? type.ToString() ?? "Unknown"
                : "Unknown";

            // Create device entity
            var device = Device.Create(deviceId, deviceName, deviceType);

            // Extract optional location
            if (command.Data?.TryGetValue("location", out var location) == true && location is not null)
            {
                device.UpdateLocation(location.ToString());
            }

            // Extract custom properties
            if (command.Data?.TryGetValue("properties", out var props) == true &&
                props is Dictionary<string, object> properties)
            {
                foreach (var (key, value) in properties)
                {
                    device.SetProperty(key, value.ToString() ?? string.Empty);
                }
            }

            // Activate the device
            device.Activate();

            // Save to PostgreSQL
            await _deviceRepository.SaveDeviceAsync(device, cancellationToken);

            _logger.LogInformation(
                "Device {DeviceId} created successfully",
                command.DeviceId);

            // Cascade to Digital Twin sync
            return new SyncDigitalTwinCommand(
                deviceId,
                SyncOperation.CreateOrUpdate,
                new Dictionary<string, object>
                {
                    ["name"] = deviceName,
                    ["type"] = deviceType,
                    ["status"] = device.Status.ToString(),
                    ["createdAt"] = device.CreatedAt.Value
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process device created event for device {DeviceId}",
                command.DeviceId);
            throw;
        }
    }
}
