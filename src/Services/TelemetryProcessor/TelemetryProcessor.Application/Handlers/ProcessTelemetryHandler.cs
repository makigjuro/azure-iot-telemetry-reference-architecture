using IoTTelemetry.Domain.Events;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Ports;
using Wolverine;

namespace TelemetryProcessor.Application.Handlers;

/// <summary>
/// Handles processing of raw telemetry from Event Hubs.
/// Orchestrates the bronze layer storage and triggers validation pipeline.
/// </summary>
public sealed class ProcessTelemetryHandler
{
    private readonly ITelemetryStorage _storage;
    private readonly ILogger<ProcessTelemetryHandler> _logger;

    public ProcessTelemetryHandler(
        ITelemetryStorage storage,
        ILogger<ProcessTelemetryHandler> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<ValidateTelemetryCommand> Handle(
        ProcessTelemetryCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing telemetry for device {DeviceId} from partition {PartitionId}, sequence {SequenceNumber}",
            command.Reading.DeviceId,
            command.PartitionId,
            command.SequenceNumber);

        // Store raw telemetry in bronze layer (audit trail)
        await _storage.StoreBronzeAsync(command.Reading, cancellationToken);

        _logger.LogDebug(
            "Stored telemetry {TelemetryId} in bronze layer",
            command.Reading.Id);

        // Cascade to validation
        return new ValidateTelemetryCommand(command.Reading);
    }
}
