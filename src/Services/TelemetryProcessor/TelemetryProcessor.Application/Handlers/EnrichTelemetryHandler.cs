using IoTTelemetry.Domain.Events;
using Microsoft.Extensions.Logging;
using TelemetryProcessor.Application.Commands;
using TelemetryProcessor.Application.Ports;
using Wolverine;

namespace TelemetryProcessor.Application.Handlers;

/// <summary>
/// Handles enrichment of telemetry with device metadata.
/// </summary>
public sealed class EnrichTelemetryHandler
{
    private readonly IDeviceMetadataRepository _metadataRepository;
    private readonly ILogger<EnrichTelemetryHandler> _logger;

    public EnrichTelemetryHandler(
        IDeviceMetadataRepository metadataRepository,
        ILogger<EnrichTelemetryHandler> logger)
    {
        _metadataRepository = metadataRepository;
        _logger = logger;
    }

    public async Task<(TelemetryEnrichedEvent, StoreTelemetryCommand)> Handle(
        EnrichTelemetryCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Enriching telemetry {TelemetryId} for device {DeviceId}",
            command.Reading.Id,
            command.Reading.DeviceId);

        // Fetch device metadata (location, model, etc.)
        var metadata = await _metadataRepository.GetMetadataAsync(
            command.Reading.DeviceId,
            cancellationToken);

        if (metadata == null)
        {
            _logger.LogWarning(
                "No metadata found for device {DeviceId}, using empty metadata",
                command.Reading.DeviceId);

            metadata = new Dictionary<string, string>();
        }

        _logger.LogDebug(
            "Enriched telemetry {TelemetryId} with {MetadataCount} metadata fields",
            command.Reading.Id,
            metadata.Count);

        return (
            new TelemetryEnrichedEvent(
                command.Reading.DeviceId,
                command.Reading.Id,
                metadata),
            new StoreTelemetryCommand(command.Reading, metadata));
    }
}
