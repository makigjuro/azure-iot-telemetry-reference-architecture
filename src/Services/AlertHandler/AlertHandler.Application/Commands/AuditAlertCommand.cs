using IoTTelemetry.Domain.Entities;

namespace AlertHandler.Application.Commands;

/// <summary>
/// Command to audit an alert in PostgreSQL for compliance and tracking.
/// </summary>
public sealed record AuditAlertCommand(
    Alert Alert,
    bool CommandSent,
    string? CommandResult = null);
