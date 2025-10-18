namespace IoTTelemetry.Domain.ValueObjects;

/// <summary>
/// Alert severity levels.
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning - requires attention.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error - requires immediate attention.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical - system failure or data loss.
    /// </summary>
    Critical = 3
}
