using IoTTelemetry.Domain.Entities;

namespace TelemetryProcessor.Application.Commands;

/// <summary>
/// Command to process raw telemetry from Event Hubs.
/// This is the entry point for the cold path processing pipeline.
/// </summary>
public sealed record ProcessTelemetryCommand(
    TelemetryReading Reading,
    string PartitionId,
    long SequenceNumber);
