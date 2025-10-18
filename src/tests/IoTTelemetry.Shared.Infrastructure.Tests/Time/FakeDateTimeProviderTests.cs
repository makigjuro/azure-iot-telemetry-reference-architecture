using IoTTelemetry.Shared.Infrastructure.Tests.Fakes;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Time;

public class FakeDateTimeProviderTests
{
    [Fact]
    public void Constructor_WithoutParameters_ShouldSetDefaultTime()
    {
        // Act
        var provider = new FakeDateTimeProvider();

        // Assert
        provider.UtcNow.Should().Be(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Constructor_WithDateTime_ShouldSetProvidedTime()
    {
        // Arrange
        var expected = new DateTimeOffset(2024, 6, 15, 14, 30, 0, TimeSpan.Zero);

        // Act
        var provider = new FakeDateTimeProvider(expected);

        // Assert
        provider.UtcNow.Should().Be(expected);
    }

    [Fact]
    public void UtcNow_CanBeSet_ShouldUpdateCurrentTime()
    {
        // Arrange
        var provider = new FakeDateTimeProvider();
        var newTime = new DateTimeOffset(2024, 12, 25, 18, 0, 0, TimeSpan.Zero);

        // Act
        provider.UtcNow = newTime;

        // Assert
        provider.UtcNow.Should().Be(newTime);
    }

    [Fact]
    public void Now_ShouldReturnLocalTime()
    {
        // Arrange
        var utcTime = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var provider = new FakeDateTimeProvider(utcTime);

        // Act
        var result = provider.Now;

        // Assert
        result.Should().Be(utcTime.ToLocalTime());
    }

    [Fact]
    public void Today_ShouldReturnMidnight()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2024, 6, 15, 14, 30, 45, TimeSpan.Zero);
        var provider = new FakeDateTimeProvider(dateTime);

        // Act
        var result = provider.Today;

        // Assert
        result.Year.Should().Be(2024);
        result.Month.Should().Be(6);
        result.Day.Should().Be(15);
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Advance_WithTimeSpan_ShouldMoveTimeForward()
    {
        // Arrange
        var provider = new FakeDateTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));

        // Act
        provider.Advance(TimeSpan.FromHours(2));

        // Assert
        provider.UtcNow.Should().Be(new DateTimeOffset(2024, 1, 1, 14, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Advance_Multiple_ShouldAccumulate()
    {
        // Arrange
        var provider = new FakeDateTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));

        // Act
        provider.Advance(TimeSpan.FromHours(1));
        provider.Advance(TimeSpan.FromMinutes(30));
        provider.Advance(TimeSpan.FromSeconds(45));

        // Assert
        provider.UtcNow.Should().Be(new DateTimeOffset(2024, 1, 1, 13, 30, 45, TimeSpan.Zero));
    }

    [Fact]
    public void SetUtcNow_ShouldUpdateCurrentTime()
    {
        // Arrange
        var provider = new FakeDateTimeProvider();
        var newTime = new DateTimeOffset(2025, 3, 15, 10, 20, 30, TimeSpan.Zero);

        // Act
        provider.SetUtcNow(newTime);

        // Assert
        provider.UtcNow.Should().Be(newTime);
    }

    [Fact]
    public void Reset_ShouldRestoreDefaultTime()
    {
        // Arrange
        var provider = new FakeDateTimeProvider();
        provider.Advance(TimeSpan.FromDays(100));

        // Act
        provider.Reset();

        // Assert
        provider.UtcNow.Should().Be(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void FakeProvider_InTest_ShouldAllowTimeControl()
    {
        // Arrange
        var provider = new FakeDateTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        // Simulate processing over 24 hours
        var readings = new List<DateTimeOffset>();

        // Act
        for (int i = 0; i < 24; i++)
        {
            readings.Add(provider.UtcNow);
            provider.Advance(TimeSpan.FromHours(1));
        }

        // Assert
        readings.Should().HaveCount(24);
        readings.First().Should().Be(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        readings.Last().Should().Be(new DateTimeOffset(2024, 1, 1, 23, 0, 0, TimeSpan.Zero));
        provider.UtcNow.Should().Be(new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero));
    }
}
