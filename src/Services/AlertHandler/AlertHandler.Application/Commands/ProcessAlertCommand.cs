using IoTTelemetry.Domain.Entities;

namespace AlertHandler.Application.Commands;

/// <summary>
/// Command to process an alert received from Stream Analytics via Service Bus.
/// This is the entry point for the hot path alert processing pipeline.
/// </summary>
public sealed record ProcessAlertCommand(
    Alert Alert,
    string MessageId,
    DateTimeOffset EnqueuedTime);
