using IoTTelemetry.Domain.Entities;

namespace TelemetryProcessor.Application.Commands;

/// <summary>
/// Command to validate telemetry reading.
/// </summary>
public sealed record ValidateTelemetryCommand(TelemetryReading Reading);
