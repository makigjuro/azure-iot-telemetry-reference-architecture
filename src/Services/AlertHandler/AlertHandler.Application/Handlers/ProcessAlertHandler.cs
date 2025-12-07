using AlertHandler.Application.Commands;
using AlertHandler.Application.Ports;
using Microsoft.Extensions.Logging;

namespace AlertHandler.Application.Handlers;

/// <summary>
/// Handles processing of alerts from Service Bus.
/// Checks for duplicates (idempotency) and cascades to device command sending.
/// </summary>
public sealed class ProcessAlertHandler
{
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<ProcessAlertHandler> _logger;

    public ProcessAlertHandler(
        IAlertRepository alertRepository,
        ILogger<ProcessAlertHandler> logger)
    {
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task<SendDeviceCommandCommand?> Handle(
        ProcessAlertCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing alert {AlertId} for device {DeviceId} with severity {Severity}",
            command.Alert.Id,
            command.Alert.DeviceId,
            command.Alert.Severity);

        // Check for duplicate processing (idempotency)
        var existingAlert = await _alertRepository.GetAlertByIdAsync(
            command.Alert.Id,
            cancellationToken);

        if (existingAlert is not null)
        {
            _logger.LogWarning(
                "Alert {AlertId} has already been processed. Skipping duplicate.",
                command.Alert.Id);
            return null; // Stop processing
        }

        // Determine if this alert requires a device command
        if (!command.Alert.RequiresImmediateAction())
        {
            _logger.LogInformation(
                "Alert {AlertId} with severity {Severity} does not require immediate action",
                command.Alert.Id,
                command.Alert.Severity);

            // Audit only
            return null;
        }

        // Build device command payload
        var payload = new Dictionary<string, object>
        {
            ["alertId"] = command.Alert.Id.ToString(),
            ["severity"] = command.Alert.Severity.ToString(),
            ["message"] = command.Alert.Message,
            ["timestamp"] = command.Alert.Timestamp.Value
        };

        // Add metadata to payload
        foreach (var (key, value) in command.Alert.Metadata)
        {
            payload[key] = value;
        }

        _logger.LogDebug(
            "Cascading to device command for alert {AlertId}",
            command.Alert.Id);

        // Cascade to device command sending
        return new SendDeviceCommandCommand(
            command.Alert.DeviceId,
            "HandleAlert",
            payload,
            command.Alert.Id);
    }
}
