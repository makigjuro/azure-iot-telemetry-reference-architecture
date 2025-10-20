using System.Diagnostics;

namespace IoTTelemetry.Shared.Infrastructure.Observability;

/// <summary>
/// Provides a centralized ActivitySource for distributed tracing across the application.
/// This is a singleton pattern to ensure a single ActivitySource instance is used.
/// </summary>
public static class ActivitySourceProvider
{
    private static readonly ActivitySource _activitySource = new(
        TelemetryConstants.ActivitySourceName,
        version: typeof(ActivitySourceProvider).Assembly.GetName().Version?.ToString() ?? "1.0.0");

    /// <summary>
    /// Gets the global ActivitySource for the application.
    /// </summary>
    public static ActivitySource ActivitySource => _activitySource;

    /// <summary>
    /// Starts a new activity with the specified name.
    /// Returns null if no listeners are registered (common in tests).
    /// </summary>
    /// <param name="name">The activity name (use constants from <see cref="TelemetryConstants.Activities"/>).</param>
    /// <param name="kind">The activity kind (default: Internal).</param>
    /// <returns>The created activity, or null if no listeners are registered.</returns>
    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return _activitySource.StartActivity(name, kind);
    }

    /// <summary>
    /// Starts a new activity with the specified name and parent context.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="kind">The activity kind.</param>
    /// <param name="parentContext">The parent activity context.</param>
    /// <returns>The created activity, or null if no listeners are registered.</returns>
    public static Activity? StartActivity(
        string name,
        ActivityKind kind,
        ActivityContext parentContext)
    {
        return _activitySource.StartActivity(name, kind, parentContext);
    }

    /// <summary>
    /// Starts a new activity with tags.
    /// </summary>
    /// <param name="name">The activity name.</param>
    /// <param name="kind">The activity kind.</param>
    /// <param name="tags">The tags to add to the activity.</param>
    /// <returns>The created activity, or null if no listeners are registered.</returns>
    public static Activity? StartActivity(
        string name,
        ActivityKind kind,
        IEnumerable<KeyValuePair<string, object?>> tags)
    {
        return _activitySource.StartActivity(name, kind, default(ActivityContext), tags);
    }

    /// <summary>
    /// Adds standard tags to an activity (device ID, correlation ID, etc.).
    /// </summary>
    /// <param name="activity">The activity to tag.</param>
    /// <param name="deviceId">Optional device ID.</param>
    /// <param name="correlationId">Optional correlation ID.</param>
    /// <param name="additionalTags">Optional additional tags.</param>
    public static void AddStandardTags(
        Activity? activity,
        string? deviceId = null,
        string? correlationId = null,
        IEnumerable<KeyValuePair<string, object?>>? additionalTags = null)
    {
        if (activity == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(deviceId))
        {
            activity.SetTag(TelemetryConstants.Tags.DeviceId, deviceId);
        }

        if (!string.IsNullOrEmpty(correlationId))
        {
            activity.SetTag(TelemetryConstants.Tags.CorrelationId, correlationId);
        }

        if (additionalTags != null)
        {
            foreach (var tag in additionalTags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }
    }

    /// <summary>
    /// Records an exception on the current activity.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="exception">The exception to record.</param>
    public static void RecordException(Activity? activity, Exception exception)
    {
        if (activity == null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag(TelemetryConstants.Tags.ErrorType, exception.GetType().Name);
        activity.SetTag(TelemetryConstants.Tags.ErrorMessage, exception.Message);
        activity.SetTag(TelemetryConstants.Tags.ExceptionStackTrace, exception.StackTrace);

        // Add exception event
        var tags = new ActivityTagsCollection
        {
            { TelemetryConstants.Tags.ErrorType, exception.GetType().FullName },
            { TelemetryConstants.Tags.ErrorMessage, exception.Message },
            { TelemetryConstants.Tags.ExceptionStackTrace, exception.StackTrace }
        };

        activity.AddEvent(new ActivityEvent("exception", DateTimeOffset.UtcNow, tags));
    }

    /// <summary>
    /// Sets the activity status to OK.
    /// </summary>
    /// <param name="activity">The activity.</param>
    public static void SetSuccess(Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    /// <summary>
    /// Sets the activity status to Error with a description.
    /// </summary>
    /// <param name="activity">The activity.</param>
    /// <param name="errorDescription">The error description.</param>
    public static void SetError(Activity? activity, string errorDescription)
    {
        activity?.SetStatus(ActivityStatusCode.Error, errorDescription);
    }
}
