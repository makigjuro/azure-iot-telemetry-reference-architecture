using AlertHandler.Application.Commands;
using AlertHandler.Application.Ports;
using Microsoft.Extensions.Logging;

namespace AlertHandler.Application.Handlers;

/// <summary>
/// Handles auditing of alerts to PostgreSQL for compliance tracking.
/// Final step in the alert processing pipeline.
/// </summary>
public sealed class AuditAlertHandler
{
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<AuditAlertHandler> _logger;

    public AuditAlertHandler(
        IAlertRepository alertRepository,
        ILogger<AuditAlertHandler> logger)
    {
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task Handle(
        AuditAlertCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auditing alert {AlertId} for device {DeviceId}. Command sent: {CommandSent}",
            command.Alert.Id,
            command.Alert.DeviceId,
            command.CommandSent);

        await _alertRepository.SaveAlertAsync(
            command.Alert,
            command.CommandSent,
            command.CommandResult,
            cancellationToken);

        _logger.LogDebug(
            "Alert {AlertId} audit saved to PostgreSQL",
            command.Alert.Id);
    }
}
