using IoTTelemetry.Domain.Entities;

namespace TelemetryProcessor.Application.Commands;

/// <summary>
/// Command to store telemetry to bronze and silver layers.
/// </summary>
public sealed record StoreTelemetryCommand(
    TelemetryReading Reading,
    Dictionary<string, string>? EnrichedMetadata = null);
