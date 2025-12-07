using AlertHandler.Application.Ports;
using IoTTelemetry.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Infrastructure.Database;

namespace AlertHandler.Infrastructure.Database;

/// <summary>
/// PostgreSQL repository for alert audit logging.
/// Reuses the shared IoTTelemetryDbContext.
/// </summary>
public sealed class AlertRepository : IAlertRepository
{
    private readonly IoTTelemetryDbContext _dbContext;
    private readonly ILogger<AlertRepository> _logger;

    public AlertRepository(
        IoTTelemetryDbContext dbContext,
        ILogger<AlertRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SaveAlertAsync(
        Alert alert,
        bool commandSent,
        string? commandResult = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Add command execution metadata
            alert.AddMetadata("commandSent", commandSent);
            if (commandResult is not null)
            {
                alert.AddMetadata("commandResult", commandResult);
            }
            alert.AddMetadata("processedAt", DateTimeOffset.UtcNow);

            _dbContext.Alerts.Add(alert);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Saved alert {AlertId} for device {DeviceId} to PostgreSQL",
                alert.Id,
                alert.DeviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save alert {AlertId} to PostgreSQL",
                alert.Id);
            throw;
        }
    }

    public async Task<Alert?> GetAlertByIdAsync(
        Guid alertId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Alerts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve alert {AlertId} from PostgreSQL",
                alertId);
            throw;
        }
    }
}
