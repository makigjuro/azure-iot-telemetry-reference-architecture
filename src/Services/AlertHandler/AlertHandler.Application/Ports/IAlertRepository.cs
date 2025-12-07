using IoTTelemetry.Domain.Entities;

namespace AlertHandler.Application.Ports;

/// <summary>
/// Port for persisting alert audit logs to PostgreSQL.
/// </summary>
public interface IAlertRepository
{
    /// <summary>
    /// Stores an alert audit record in PostgreSQL.
    /// </summary>
    /// <param name="alert">Alert to audit</param>
    /// <param name="commandSent">Whether a C2D command was sent</param>
    /// <param name="commandResult">Result of the C2D command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveAlertAsync(
        Alert alert,
        bool commandSent,
        string? commandResult = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an alert by ID to check for duplicate processing (idempotency).
    /// </summary>
    /// <param name="alertId">Alert ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Alert if exists, null otherwise</returns>
    Task<Alert?> GetAlertByIdAsync(Guid alertId, CancellationToken cancellationToken = default);
}
