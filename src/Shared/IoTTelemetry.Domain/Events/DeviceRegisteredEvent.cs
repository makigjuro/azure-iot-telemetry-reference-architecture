using IoTTelemetry.Domain.Common;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Events;

/// <summary>
/// Domain event raised when a device is registered.
/// </summary>
public sealed record DeviceRegisteredEvent(
    DeviceId DeviceId,
    string Name,
    string Type) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
