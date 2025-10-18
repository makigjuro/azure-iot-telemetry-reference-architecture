using IoTTelemetry.Shared.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace IoTTelemetry.Shared.Infrastructure.Tests.Time;

public class DateTimeProviderExtensionsTests
{
    [Fact]
    public void AddDateTimeProvider_ShouldRegisterSystemDateTimeProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDateTimeProvider();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<IDateTimeProvider>();
        provider.Should().NotBeNull();
        provider.Should().BeOfType<SystemDateTimeProvider>();
    }

    [Fact]
    public void AddDateTimeProvider_ShouldRegisterAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDateTimeProvider();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider1 = serviceProvider.GetService<IDateTimeProvider>();
        var provider2 = serviceProvider.GetService<IDateTimeProvider>();

        provider1.Should().BeSameAs(provider2);
    }

    [Fact]
    public void AddDateTimeProvider_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDateTimeProvider();

        // Assert
        result.Should().BeSameAs(services);
    }
}
