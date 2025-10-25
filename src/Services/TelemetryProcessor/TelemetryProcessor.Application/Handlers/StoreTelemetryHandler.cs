using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Ports;

namespace TelemetryProcessor.Application.Handlers;

/// <summary>
/// Handles storing telemetry to silver layer (validated + enriched).
/// </summary>
public sealed class StoreTelemetryHandler
{
    private readonly ITelemetryStorage _storage;
    private readonly ILogger<StoreTelemetryHandler> _logger;

    public StoreTelemetryHandler(
        ITelemetryStorage storage,
        ILogger<StoreTelemetryHandler> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task Handle(
        StoreTelemetryCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Storing telemetry {TelemetryId} to silver layer",
            command.Reading.Id);

        // Store validated + enriched telemetry in silver layer
        await _storage.StoreSilverAsync(
            command.Reading,
            command.EnrichedMetadata ?? new Dictionary<string, string>(),
            cancellationToken);

        _logger.LogInformation(
            "Successfully processed and stored telemetry {TelemetryId} for device {DeviceId}",
            command.Reading.Id,
            command.Reading.DeviceId);
    }
}
