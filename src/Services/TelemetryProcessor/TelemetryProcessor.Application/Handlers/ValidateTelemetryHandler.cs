using IoTTelemetry.Domain.Events;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Ports;
using Wolverine;

namespace TelemetryProcessor.Application.Handlers;

/// <summary>
/// Handles validation of telemetry readings.
/// </summary>
public sealed class ValidateTelemetryHandler
{
    private readonly ITelemetryValidator _validator;
    private readonly ILogger<ValidateTelemetryHandler> _logger;

    public ValidateTelemetryHandler(
        ITelemetryValidator validator,
        ILogger<ValidateTelemetryHandler> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public async Task<(TelemetryValidatedEvent, EnrichTelemetryCommand?)> Handle(
        ValidateTelemetryCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Validating telemetry {TelemetryId} for device {DeviceId}",
            command.Reading.Id,
            command.Reading.DeviceId);

        var validationResult = await _validator.ValidateAsync(command.Reading);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "Telemetry {TelemetryId} validation failed: {Error}",
                command.Reading.Id,
                validationResult.Error);

            command.Reading.MarkAsInvalid(validationResult.Error!);

            return (
                new TelemetryValidatedEvent(
                    command.Reading.DeviceId,
                    command.Reading.Id,
                    false,
                    validationResult.Error),
                null); // Don't cascade invalid telemetry
        }

        _logger.LogDebug(
            "Telemetry {TelemetryId} validated successfully",
            command.Reading.Id);

        return (
            new TelemetryValidatedEvent(
                command.Reading.DeviceId,
                command.Reading.Id,
                true,
                null),
            new EnrichTelemetryCommand(command.Reading));
    }
}
