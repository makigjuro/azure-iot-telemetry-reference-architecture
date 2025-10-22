using Serilog.Core;
using Serilog.Events;

namespace IoTTelemetry.Shared.Infrastructure.Logging;

/// <summary>
/// Enriches log events with service context information (service name, version, environment).
/// </summary>
public class ServiceContextEnricher : ILogEventEnricher
{
    private readonly LogEventProperty _serviceNameProperty;
    private readonly LogEventProperty _serviceVersionProperty;
    private readonly LogEventProperty? _environmentProperty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceContextEnricher"/> class.
    /// </summary>
    /// <param name="serviceName">The service name.</param>
    /// <param name="serviceVersion">The service version.</param>
    /// <param name="environment">The deployment environment (optional).</param>
    public ServiceContextEnricher(string serviceName, string serviceVersion, string? environment = null)
    {
        _serviceNameProperty = new LogEventProperty("ServiceName", new ScalarValue(serviceName));
        _serviceVersionProperty = new LogEventProperty("ServiceVersion", new ScalarValue(serviceVersion));

        if (!string.IsNullOrEmpty(environment))
        {
            _environmentProperty = new LogEventProperty("Environment", new ScalarValue(environment));
        }
    }

    /// <summary>
    /// Enriches the log event with service context properties.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">The property factory.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(_serviceNameProperty);
        logEvent.AddPropertyIfAbsent(_serviceVersionProperty);

        if (_environmentProperty != null)
        {
            logEvent.AddPropertyIfAbsent(_environmentProperty);
        }
    }
}
