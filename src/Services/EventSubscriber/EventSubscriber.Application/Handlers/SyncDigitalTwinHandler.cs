using EventSubscriber.Application.Commands;
using EventSubscriber.Application.Ports;
using Microsoft.Extensions.Logging;

namespace EventSubscriber.Application.Handlers;

/// <summary>
/// Handles Digital Twin synchronization commands.
/// Creates/updates or deletes digital twins in Azure Digital Twins.
/// </summary>
public sealed class SyncDigitalTwinHandler
{
    private readonly IDigitalTwinService _digitalTwinService;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<SyncDigitalTwinHandler> _logger;

    public SyncDigitalTwinHandler(
        IDigitalTwinService digitalTwinService,
        IDeviceRepository deviceRepository,
        ILogger<SyncDigitalTwinHandler> logger)
    {
        _digitalTwinService = digitalTwinService;
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    public async Task Handle(
        SyncDigitalTwinCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Syncing digital twin for device {DeviceId} with operation {Operation}",
                command.DeviceId,
                command.Operation);

            bool success;

            if (command.Operation == SyncOperation.Delete)
            {
                success = await _digitalTwinService.DeleteTwinAsync(
                    command.DeviceId,
                    cancellationToken);

                if (success)
                {
                    _logger.LogInformation(
                        "Digital twin deleted successfully for device {DeviceId}",
                        command.DeviceId);
                }
                else
                {
                    _logger.LogWarning(
                        "Digital twin deletion failed or twin does not exist for device {DeviceId}",
                        command.DeviceId);
                }
            }
            else
            {
                // Retrieve device for full sync
                var device = await _deviceRepository.GetDeviceByIdAsync(
                    command.DeviceId,
                    cancellationToken);

                if (device is null)
                {
                    _logger.LogWarning(
                        "Cannot sync digital twin: device {DeviceId} not found in repository",
                        command.DeviceId);
                    return;
                }

                success = await _digitalTwinService.CreateOrUpdateTwinAsync(
                    device,
                    cancellationToken);

                if (success)
                {
                    _logger.LogInformation(
                        "Digital twin created/updated successfully for device {DeviceId}",
                        command.DeviceId);
                }
                else
                {
                    _logger.LogError(
                        "Digital twin creation/update failed for device {DeviceId}",
                        command.DeviceId);
                    throw new InvalidOperationException(
                        $"Failed to sync digital twin for device {command.DeviceId}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to sync digital twin for device {DeviceId}",
                command.DeviceId);
            throw;
        }
    }
}
