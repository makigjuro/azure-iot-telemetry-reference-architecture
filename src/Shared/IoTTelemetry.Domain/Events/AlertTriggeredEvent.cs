using IoTTelemetry.Domain.Common;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Events;

/// <summary>
/// Domain event raised when an alert is triggered.
/// </summary>
public sealed record AlertTriggeredEvent(
    DeviceId DeviceId,
    AlertSeverity Severity,
    string Message) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
