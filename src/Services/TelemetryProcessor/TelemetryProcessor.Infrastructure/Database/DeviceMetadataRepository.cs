using IoTTelemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Infrastructure.Database;

/// <summary>
/// Repository for retrieving device metadata from PostgreSQL.
/// For now, returns mock data. Will be implemented with EF Core later.
/// </summary>
public sealed class DeviceMetadataRepository : IDeviceMetadataRepository
{
    private readonly ILogger<DeviceMetadataRepository> _logger;

    public DeviceMetadataRepository(ILogger<DeviceMetadataRepository> logger)
    {
        _logger = logger;
    }

    public Task<Dictionary<string, string>?> GetMetadataAsync(
        DeviceId deviceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching metadata for device {DeviceId}", deviceId);

        // TODO: Implement actual EF Core query when device schema is ready
        // For now, return mock metadata
        var metadata = new Dictionary<string, string>
        {
            ["location"] = "Warehouse-A",
            ["model"] = "IoT-Sensor-v2",
            ["manufacturer"] = "Contoso",
            ["firmwareVersion"] = "1.2.3"
        };

        return Task.FromResult<Dictionary<string, string>?>(metadata);
    }
}
