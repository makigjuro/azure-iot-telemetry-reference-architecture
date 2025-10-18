using IoTTelemetry.Shared.Infrastructure.Time;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Fakes;

/// <summary>
/// Fake implementation of IDateTimeProvider for testing.
/// Allows controlling time in tests.
/// </summary>
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    private DateTimeOffset _utcNow;

    public FakeDateTimeProvider()
    {
        _utcNow = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    }

    public FakeDateTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public DateTimeOffset UtcNow
    {
        get => _utcNow;
        set => _utcNow = value;
    }

    public DateTimeOffset Now => _utcNow.ToLocalTime();

    public DateTimeOffset Today => _utcNow.Date;

    /// <summary>
    /// Advances time by the specified duration.
    /// </summary>
    public void Advance(TimeSpan duration)
    {
        _utcNow = _utcNow.Add(duration);
    }

    /// <summary>
    /// Sets the current time.
    /// </summary>
    public void SetUtcNow(DateTimeOffset dateTime)
    {
        _utcNow = dateTime;
    }

    /// <summary>
    /// Resets to default time (2024-01-01 12:00:00 UTC).
    /// </summary>
    public void Reset()
    {
        _utcNow = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    }
}
