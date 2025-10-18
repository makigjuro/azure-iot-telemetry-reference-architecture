using Microsoft.Extensions.DependencyInjection;

namespace IoTTelemetry.Shared.Infrastructure.Time;

/// <summary>
/// Extension methods for registering IDateTimeProvider in DI container.
/// </summary>
public static class DateTimeProviderExtensions
{
    /// <summary>
    /// Registers the system date/time provider as a singleton.
    /// </summary>
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        return services;
    }
}
