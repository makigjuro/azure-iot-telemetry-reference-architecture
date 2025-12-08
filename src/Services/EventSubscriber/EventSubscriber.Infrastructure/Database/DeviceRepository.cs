using EventSubscriber.Application.Ports;
using IoTTelemetry.Domain.Entities;
using IoTTelemetry.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Infrastructure.Database;

namespace EventSubscriber.Infrastructure.Database;

/// <summary>
/// PostgreSQL repository for device metadata.
/// </summary>
public sealed class DeviceRepository : IDeviceRepository
{
    private readonly IoTTelemetryDbContext _dbContext;
    private readonly ILogger<DeviceRepository> _logger;

    public DeviceRepository(
        IoTTelemetryDbContext dbContext,
        ILogger<DeviceRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SaveDeviceAsync(
        Device device,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingDevice = await _dbContext.Devices
                .FirstOrDefaultAsync(d => d.Id == device.Id, cancellationToken);

            if (existingDevice is null)
            {
                _dbContext.Devices.Add(device);
                _logger.LogDebug("Adding new device {DeviceId} to database", device.Id);
            }
            else
            {
                _dbContext.Entry(existingDevice).CurrentValues.SetValues(device);
                _logger.LogDebug("Updating existing device {DeviceId} in database", device.Id);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save device {DeviceId} to database",
                device.Id);
            throw;
        }
    }

    public async Task<Device?> GetDeviceByIdAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve device {DeviceId} from database",
                deviceId);
            throw;
        }
    }

    public async Task DeleteDeviceAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var device = await _dbContext.Devices
                .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);

            if (device is not null)
            {
                _dbContext.Devices.Remove(device);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Deleted device {DeviceId} from database", deviceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete device {DeviceId} from database",
                deviceId);
            throw;
        }
    }

    public async Task<bool> DeviceExistsAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Devices
                .AnyAsync(d => d.Id == deviceId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to check device existence for {DeviceId}",
                deviceId);
            throw;
        }
    }
}
