using IoTTelemetry.Domain.ValueObjects;

namespace EventSubscriber.Application.Commands;

/// <summary>
/// Command to synchronize a device with Azure Digital Twins.
/// Cascaded from device lifecycle event handlers.
/// </summary>
public sealed record SyncDigitalTwinCommand(
    DeviceId DeviceId,
    SyncOperation Operation,
    Dictionary<string, object>? Metadata = null);

/// <summary>
/// Type of Digital Twin synchronization operation.
/// </summary>
public enum SyncOperation
{
    CreateOrUpdate,
    Delete
}
