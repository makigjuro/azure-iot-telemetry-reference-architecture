using IoTTelemetry.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Infrastructure.Database;

/// <summary>
/// Repository for retrieving device metadata from PostgreSQL.
/// </summary>
public sealed class DeviceMetadataRepository : IDeviceMetadataRepository
{
    private readonly IoTTelemetryDbContext _dbContext;
    private readonly ILogger<DeviceMetadataRepository> _logger;

    public DeviceMetadataRepository(
        IoTTelemetryDbContext dbContext,
        ILogger<DeviceMetadataRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Dictionary<string, string>?> GetMetadataAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching metadata for device {DeviceId}", deviceId);

        var device = await _dbContext.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);

        if (device == null)
        {
            _logger.LogWarning("Device {DeviceId} not found in database", deviceId);
            return null;
        }

        // Build metadata dictionary from device properties
        var metadata = new Dictionary<string, string>(device.Properties)
        {
            ["deviceName"] = device.Name,
            ["deviceType"] = device.Type,
            ["deviceStatus"] = device.Status.ToString()
        };

        // Add location if available
        if (!string.IsNullOrWhiteSpace(device.Location))
        {
            metadata["location"] = device.Location;
        }

        _logger.LogDebug(
            "Retrieved {MetadataCount} metadata fields for device {DeviceId}",
            metadata.Count,
            deviceId);

        return metadata;
    }
}
