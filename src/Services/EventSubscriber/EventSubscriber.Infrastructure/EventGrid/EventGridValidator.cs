using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace EventSubscriber.Infrastructure.EventGrid;

/// <summary>
/// Validates Event Grid webhook subscription requests.
/// </summary>
public sealed class EventGridValidator
{
    private readonly ILogger<EventGridValidator> _logger;

    public EventGridValidator(ILogger<EventGridValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates Event Grid subscription and returns validation response if needed.
    /// </summary>
    /// <returns>Validation code if this is a subscription validation request, null otherwise.</returns>
    public string? ValidateSubscription(string requestBody)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(requestBody);
            var root = jsonDoc.RootElement;

            // Check if this is an array of events
            if (root.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("Event Grid request is not an array");
                return null;
            }

            // Get the first event
            if (root.GetArrayLength() == 0)
            {
                _logger.LogWarning("Event Grid request contains no events");
                return null;
            }

            var firstEvent = root[0];

            if (!firstEvent.TryGetProperty("eventType", out var eventType))
            {
                _logger.LogWarning("Event Grid event missing eventType property");
                return null;
            }

            var eventTypeString = eventType.GetString();

            // Check if this is a subscription validation event
            if (eventTypeString == "Microsoft.EventGrid.SubscriptionValidationEvent")
            {
                _logger.LogInformation("Received Event Grid subscription validation request");

                if (firstEvent.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("validationCode", out var validationCode))
                {
                    var code = validationCode.GetString();
                    _logger.LogInformation("Returning validation code for Event Grid subscription");
                    return code;
                }

                _logger.LogWarning("Subscription validation event missing validationCode");
            }

            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Event Grid request");
            return null;
        }
    }

    /// <summary>
    /// Parses Event Grid events from request body.
    /// </summary>
    public List<EventGridEvent> ParseEvents(string requestBody)
    {
        try
        {
            var events = JsonSerializer.Deserialize<List<EventGridEvent>>(requestBody);
            return events ?? new List<EventGridEvent>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Event Grid events");
            return new List<EventGridEvent>();
        }
    }
}

/// <summary>
/// Represents an Event Grid event.
/// </summary>
public sealed record EventGridEvent(
    string Id,
    string EventType,
    string Subject,
    DateTimeOffset EventTime,
    string DataVersion,
    Dictionary<string, object>? Data);
