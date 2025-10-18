using IoTTelemetry.Domain.Common;
using IoTTelemetry.Domain.Events;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Entities;

/// <summary>
/// Device aggregate root.
/// Represents an IoT device with lifecycle management and state tracking.
/// </summary>
public sealed class Device : AggregateRoot<DeviceId>
{
    private Device(DeviceId id, string name, string type) : base(id)
    {
        Name = name;
        Type = type;
        Status = DeviceStatus.Registered;
        CreatedAt = Timestamp.Now();
    }

    public string Name { get; private set; }
    public string Type { get; private set; }
    public DeviceStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private init; }
    public Timestamp? LastSeenAt { get; private set; }
    public Timestamp? LastModifiedAt { get; private set; }
    public string? Location { get; private set; }
    public Dictionary<string, string> Properties { get; private set; } = new();

    /// <summary>
    /// Creates a new device.
    /// </summary>
    public static Device Create(DeviceId deviceId, string name, string type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Device name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Device type cannot be empty.", nameof(type));
        }

        var device = new Device(deviceId, name, type);
        device.RaiseDomainEvent(new DeviceRegisteredEvent(deviceId, name, type));

        return device;
    }

    /// <summary>
    /// Activates the device for operation.
    /// </summary>
    public void Activate()
    {
        if (Status == DeviceStatus.Decommissioned)
        {
            throw new InvalidOperationException("Cannot activate a decommissioned device.");
        }

        if (Status == DeviceStatus.Active)
        {
            return; // Already active
        }

        Status = DeviceStatus.Active;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Deactivates the device temporarily.
    /// </summary>
    public void Deactivate()
    {
        if (Status == DeviceStatus.Decommissioned)
        {
            throw new InvalidOperationException("Cannot deactivate a decommissioned device.");
        }

        Status = DeviceStatus.Inactive;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Disables the device (prevents telemetry).
    /// </summary>
    public void Disable()
    {
        if (Status == DeviceStatus.Decommissioned)
        {
            throw new InvalidOperationException("Cannot disable a decommissioned device.");
        }

        Status = DeviceStatus.Disabled;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Puts device into maintenance mode.
    /// </summary>
    public void StartMaintenance()
    {
        if (Status == DeviceStatus.Decommissioned)
        {
            throw new InvalidOperationException("Cannot put a decommissioned device into maintenance.");
        }

        Status = DeviceStatus.Maintenance;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Ends maintenance mode and reactivates device.
    /// </summary>
    public void EndMaintenance()
    {
        if (Status != DeviceStatus.Maintenance)
        {
            throw new InvalidOperationException("Device is not in maintenance mode.");
        }

        Status = DeviceStatus.Active;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Decommissions the device (final state).
    /// </summary>
    public void Decommission()
    {
        Status = DeviceStatus.Decommissioned;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Updates the last seen timestamp.
    /// </summary>
    public void RecordActivity()
    {
        LastSeenAt = Timestamp.Now();
    }

    /// <summary>
    /// Updates device name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Device name cannot be empty.", nameof(name));
        }

        Name = name;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Updates device location.
    /// </summary>
    public void UpdateLocation(string? location)
    {
        Location = location;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Sets a custom property.
    /// </summary>
    public void SetProperty(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Property key cannot be empty.", nameof(key));
        }

        Properties[key] = value;
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Removes a custom property.
    /// </summary>
    public void RemoveProperty(string key)
    {
        Properties.Remove(key);
        LastModifiedAt = Timestamp.Now();
    }

    /// <summary>
    /// Checks if device can send telemetry.
    /// </summary>
    public bool CanSendTelemetry()
    {
        return Status is DeviceStatus.Active or DeviceStatus.Maintenance;
    }
}
