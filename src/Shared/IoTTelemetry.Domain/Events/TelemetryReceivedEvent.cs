using IoTTelemetry.Domain.Common;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Events;

/// <summary>
/// Domain event raised when telemetry is received from a device.
/// </summary>
public sealed record TelemetryReceivedEvent(
    DeviceId DeviceId,
    Timestamp Timestamp,
    int MeasurementCount) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
