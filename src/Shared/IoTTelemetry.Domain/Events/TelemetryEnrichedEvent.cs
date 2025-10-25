using IoTTelemetry.Domain.Common;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Events;

/// <summary>
/// Domain event raised when telemetry has been enriched with device metadata.
/// </summary>
public sealed record TelemetryEnrichedEvent(
    DeviceId DeviceId,
    Guid TelemetryId,
    Dictionary<string, string> EnrichedMetadata) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
