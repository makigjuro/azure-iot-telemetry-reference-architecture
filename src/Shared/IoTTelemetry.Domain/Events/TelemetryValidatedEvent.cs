using IoTTelemetry.Domain.Common;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Events;

/// <summary>
/// Domain event raised when telemetry has been validated.
/// </summary>
public sealed record TelemetryValidatedEvent(
    DeviceId DeviceId,
    Guid TelemetryId,
    bool IsValid,
    string? ValidationError) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
