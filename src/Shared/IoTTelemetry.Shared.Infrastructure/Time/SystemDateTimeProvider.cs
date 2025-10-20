namespace IoTTelemetry.Shared.Infrastructure.Time;

/// <summary>
/// Production implementation of IDateTimeProvider using system time.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTimeOffset Now => DateTimeOffset.Now;

    public DateTimeOffset Today => DateTimeOffset.UtcNow.Date;
}
