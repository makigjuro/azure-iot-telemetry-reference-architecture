namespace IoTTelemetry.Shared.Infrastructure.Result;

/// <summary>
/// Represents an error with code, message, and optional metadata.
/// Used in Result pattern for error handling without exceptions.
/// </summary>
#pragma warning disable CA1716 // "Error" is intentional naming for Result pattern
public sealed record Error
#pragma warning restore CA1716
{
    public string Code { get; }
    public string Message { get; }
    public Dictionary<string, object>? Metadata { get; }

    private Error(string code, string message, Dictionary<string, object>? metadata = null)
    {
        Code = code;
        Message = message;
        Metadata = metadata;
    }

    /// <summary>
    /// Creates a new error.
    /// </summary>
    public static Error Create(string code, string message, Dictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code cannot be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Error message cannot be empty.", nameof(message));
        }

        return new Error(code, message, metadata);
    }

    // Common error factory methods
    public static Error NotFound(string message, Dictionary<string, object>? metadata = null)
        => Create("NOT_FOUND", message, metadata);

    public static Error Validation(string message, Dictionary<string, object>? metadata = null)
        => Create("VALIDATION_ERROR", message, metadata);

    public static Error Conflict(string message, Dictionary<string, object>? metadata = null)
        => Create("CONFLICT", message, metadata);

    public static Error Unauthorized(string message, Dictionary<string, object>? metadata = null)
        => Create("UNAUTHORIZED", message, metadata);

    public static Error Forbidden(string message, Dictionary<string, object>? metadata = null)
        => Create("FORBIDDEN", message, metadata);

    public static Error Internal(string message, Dictionary<string, object>? metadata = null)
        => Create("INTERNAL_ERROR", message, metadata);

    public static Error External(string message, Dictionary<string, object>? metadata = null)
        => Create("EXTERNAL_SERVICE_ERROR", message, metadata);

    public static Error Timeout(string message, Dictionary<string, object>? metadata = null)
        => Create("TIMEOUT", message, metadata);

    public static Error RateLimited(string message, Dictionary<string, object>? metadata = null)
        => Create("RATE_LIMITED", message, metadata);

    /// <summary>
    /// Adds metadata to this error.
    /// </summary>
    public Error WithMetadata(string key, object value)
    {
        var metadata = Metadata != null
            ? new Dictionary<string, object>(Metadata)
            : new Dictionary<string, object>();

        metadata[key] = value;

        return new Error(Code, Message, metadata);
    }

    public override string ToString() => $"[{Code}] {Message}";
}
