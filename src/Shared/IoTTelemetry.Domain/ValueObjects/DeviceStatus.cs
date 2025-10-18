namespace IoTTelemetry.Domain.ValueObjects;

/// <summary>
/// Device status enumeration.
/// </summary>
public enum DeviceStatus
{
    /// <summary>
    /// Device is registered but not yet activated.
    /// </summary>
    Registered = 0,

    /// <summary>
    /// Device is active and operational.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Device is temporarily inactive.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Device is disabled and cannot send telemetry.
    /// </summary>
    Disabled = 3,

    /// <summary>
    /// Device is in maintenance mode.
    /// </summary>
    Maintenance = 4,

    /// <summary>
    /// Device is decommissioned.
    /// </summary>
    Decommissioned = 5
}
