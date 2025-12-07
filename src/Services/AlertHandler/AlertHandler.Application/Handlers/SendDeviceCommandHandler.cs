using AlertHandler.Application.Commands;
using AlertHandler.Application.Ports;
using Microsoft.Extensions.Logging;

namespace AlertHandler.Application.Handlers;

/// <summary>
/// Handles sending Cloud-to-Device commands via IoT Hub.
/// Cascades to audit logging after command delivery.
/// </summary>
public sealed class SendDeviceCommandHandler
{
    private readonly IDeviceCommandSender _commandSender;
    private readonly ILogger<SendDeviceCommandHandler> _logger;

    public SendDeviceCommandHandler(
        IDeviceCommandSender commandSender,
        ILogger<SendDeviceCommandHandler> logger)
    {
        _commandSender = commandSender;
        _logger = logger;
    }

    public async Task<AuditAlertCommand> Handle(
        SendDeviceCommandCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending C2D command '{CommandName}' to device {DeviceId} for alert {AlertId}",
            command.CommandName,
            command.DeviceId,
            command.AlertId);

        bool success;
        string? result = null;

        try
        {
            success = await _commandSender.SendCommandAsync(
                command.DeviceId,
                command.CommandName,
                command.Payload,
                cancellationToken);

            result = success ? "Command delivered successfully" : "Command delivery failed";

            _logger.LogInformation(
                "C2D command result for alert {AlertId}: {Result}",
                command.AlertId,
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send C2D command for alert {AlertId}",
                command.AlertId);

            success = false;
            result = $"Exception: {ex.Message}";
        }

        // Reconstruct Alert from command data (we need it for auditing)
        // Note: In a real scenario, we might pass the Alert through the chain
        // For now, we'll create a minimal alert for auditing purposes
        var alert = IoTTelemetry.Domain.Entities.Alert.Create(
            command.DeviceId,
            IoTTelemetry.Domain.ValueObjects.AlertSeverity.Warning, // Default, should be from original alert
            command.Payload.TryGetValue("message", out var msg) ? msg.ToString()! : "Alert",
            IoTTelemetry.Domain.ValueObjects.Timestamp.Now()
        );

        // Cascade to audit
        return new AuditAlertCommand(alert, success, result);
    }
}
