using IoTTelemetry.Domain.Entities;

namespace TelemetryProcessor.Application.Commands;

/// <summary>
/// Command to enrich telemetry with device metadata.
/// </summary>
public sealed record EnrichTelemetryCommand(TelemetryReading Reading);
