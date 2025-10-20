namespace IoTTelemetry.Shared.Infrastructure.Time;

/// <summary>
/// Abstraction for getting the current date and time.
/// Enables testable code by allowing time to be controlled in tests.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    DateTimeOffset Now { get; }

    /// <summary>
    /// Gets today's date (midnight UTC).
    /// </summary>
    DateTimeOffset Today { get; }
}
