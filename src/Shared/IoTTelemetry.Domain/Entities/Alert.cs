using IoTTelemetry.Domain.Common;
using IoTTelemetry.Domain.Events;
using IoTTelemetry.Domain.ValueObjects;

namespace IoTTelemetry.Domain.Entities;

/// <summary>
/// Represents an alert triggered by anomalous telemetry or device conditions.
/// </summary>
public sealed class Alert : Entity<Guid>
{
    private Alert(
        Guid id,
        DeviceId deviceId,
        AlertSeverity severity,
        string message,
        Timestamp timestamp) : base(id)
    {
        DeviceId = deviceId;
        Severity = severity;
        Message = message;
        Timestamp = timestamp;
        CreatedAt = Timestamp.Now();
        IsAcknowledged = false;
    }

    public DeviceId DeviceId { get; private init; }
    public AlertSeverity Severity { get; private init; }
    public string Message { get; private set; }
    public Timestamp Timestamp { get; private init; }
    public Timestamp CreatedAt { get; private init; }
    public bool IsAcknowledged { get; private set; }
    public Timestamp? AcknowledgedAt { get; private set; }
    public string? AcknowledgedBy { get; private set; }
    public string? Resolution { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    /// <summary>
    /// Creates a new alert.
    /// </summary>
    public static Alert Create(
        DeviceId deviceId,
        AlertSeverity severity,
        string message,
        Timestamp timestamp,
        Dictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Alert message cannot be empty.", nameof(message));
        }

        var id = Guid.NewGuid();
        var alert = new Alert(id, deviceId, severity, message, timestamp);

        if (metadata is not null)
        {
            alert.Metadata = metadata;
        }

        return alert;
    }

    /// <summary>
    /// Acknowledges the alert.
    /// </summary>
    public void Acknowledge(string acknowledgedBy, string? resolution = null)
    {
        if (IsAcknowledged)
        {
            throw new InvalidOperationException("Alert is already acknowledged.");
        }

        if (string.IsNullOrWhiteSpace(acknowledgedBy))
        {
            throw new ArgumentException("AcknowledgedBy cannot be empty.", nameof(acknowledgedBy));
        }

        IsAcknowledged = true;
        AcknowledgedAt = Timestamp.Now();
        AcknowledgedBy = acknowledgedBy;
        Resolution = resolution;
    }

    /// <summary>
    /// Updates the resolution notes.
    /// </summary>
    public void UpdateResolution(string resolution)
    {
        if (!IsAcknowledged)
        {
            throw new InvalidOperationException("Cannot update resolution for an unacknowledged alert.");
        }

        Resolution = resolution;
    }

    /// <summary>
    /// Adds metadata to the alert.
    /// </summary>
    public void AddMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Metadata key cannot be empty.", nameof(key));
        }

        Metadata[key] = value;
    }

    /// <summary>
    /// Checks if alert requires immediate action.
    /// </summary>
    public bool RequiresImmediateAction()
    {
        return Severity is AlertSeverity.Error or AlertSeverity.Critical && !IsAcknowledged;
    }

    /// <summary>
    /// Gets the age of the alert.
    /// </summary>
    public TimeSpan GetAge()
    {
        return DateTimeOffset.UtcNow - CreatedAt.Value;
    }
}
