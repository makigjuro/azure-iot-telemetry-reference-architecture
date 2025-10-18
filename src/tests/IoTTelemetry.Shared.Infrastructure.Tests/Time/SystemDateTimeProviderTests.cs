using IoTTelemetry.Shared.Infrastructure.Time;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Time;

public class SystemDateTimeProviderTests
{
    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        // Arrange
        var provider = new SystemDateTimeProvider();
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = provider.UtcNow;
        var after = DateTimeOffset.UtcNow;

        // Assert
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
        result.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Now_ShouldReturnCurrentLocalTime()
    {
        // Arrange
        var provider = new SystemDateTimeProvider();
        var before = DateTimeOffset.Now;

        // Act
        var result = provider.Now;
        var after = DateTimeOffset.Now;

        // Assert
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Today_ShouldReturnTodaysMidnightUtc()
    {
        // Arrange
        var provider = new SystemDateTimeProvider();
        var expected = DateTimeOffset.UtcNow.Date;

        // Act
        var result = provider.Today;

        // Assert
        result.Date.Should().Be(expected);
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void UtcNow_CalledMultipleTimes_ShouldReturnDifferentValues()
    {
        // Arrange
        var provider = new SystemDateTimeProvider();

        // Act
        var first = provider.UtcNow;
        Thread.Sleep(10); // Small delay
        var second = provider.UtcNow;

        // Assert
        second.Should().BeAfter(first);
    }
}
