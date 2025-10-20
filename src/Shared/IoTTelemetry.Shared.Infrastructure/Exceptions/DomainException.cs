namespace IoTTelemetry.Shared.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when a domain business rule is violated.
/// Represents errors that occur within the domain model.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object>? Metadata { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, string errorCode, Dictionary<string, object> metadata)
        : base(message)
    {
        ErrorCode = errorCode;
        Metadata = metadata;
    }

    /// <summary>
    /// Adds metadata to the exception.
    /// </summary>
    public DomainException WithMetadata(string key, object value)
    {
        var metadata = Metadata != null
            ? new Dictionary<string, object>(Metadata)
            : new Dictionary<string, object>();

        metadata[key] = value;

        return new DomainException(Message, ErrorCode, metadata);
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        return Metadata != null
            ? $"{baseString}\nMetadata: {string.Join(", ", Metadata.Select(kv => $"{kv.Key}={kv.Value}"))}"
            : baseString;
    }
}
