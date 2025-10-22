using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace IoTTelemetry.Shared.Infrastructure.Logging;

/// <summary>
/// Enriches log events with correlation ID from the current Activity (W3C Trace Context).
/// This ensures logs are correlated with distributed traces.
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdPropertyName = "CorrelationId";
    private const string TraceIdPropertyName = "TraceId";
    private const string SpanIdPropertyName = "SpanId";

    /// <summary>
    /// Enriches the log event with correlation information from the current Activity.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">The property factory.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;

        if (activity == null)
        {
            return;
        }

        // Add TraceId (W3C trace context)
        if (activity.TraceId != default)
        {
            var traceIdProperty = propertyFactory.CreateProperty(TraceIdPropertyName, activity.TraceId.ToString());
            logEvent.AddPropertyIfAbsent(traceIdProperty);
        }

        // Add SpanId (current span in the trace)
        if (activity.SpanId != default)
        {
            var spanIdProperty = propertyFactory.CreateProperty(SpanIdPropertyName, activity.SpanId.ToString());
            logEvent.AddPropertyIfAbsent(spanIdProperty);
        }

        // Add CorrelationId (alias for TraceId for backward compatibility)
        if (activity.TraceId != default)
        {
            var correlationIdProperty = propertyFactory.CreateProperty(CorrelationIdPropertyName, activity.TraceId.ToString());
            logEvent.AddPropertyIfAbsent(correlationIdProperty);
        }

        // If there's a baggage item with correlation ID, use that instead
        foreach (var baggage in activity.Baggage)
        {
            if (baggage.Key.Equals("correlation-id", StringComparison.OrdinalIgnoreCase))
            {
                var baggageProperty = propertyFactory.CreateProperty(CorrelationIdPropertyName, baggage.Value);
                logEvent.AddOrUpdateProperty(baggageProperty);
                break;
            }
        }
    }
}
