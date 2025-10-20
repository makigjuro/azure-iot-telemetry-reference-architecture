namespace IoTTelemetry.Shared.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when an infrastructure/external service operation fails.
/// Represents errors that occur outside the domain (databases, APIs, Azure services, etc.).
/// </summary>
public class InfrastructureException : Exception
{
    public string ErrorCode { get; }
    public string? ServiceName { get; }
    public bool IsTransient { get; }
    public Dictionary<string, object>? Metadata { get; }

    public InfrastructureException(
        string message,
        string errorCode = "INFRASTRUCTURE_ERROR",
        bool isTransient = false)
        : base(message)
    {
        ErrorCode = errorCode;
        IsTransient = isTransient;
    }

    public InfrastructureException(
        string message,
        string errorCode,
        Exception innerException,
        bool isTransient = false)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        IsTransient = isTransient;
    }

    public InfrastructureException(
        string message,
        string errorCode,
        string serviceName,
        bool isTransient,
        Dictionary<string, object>? metadata = null)
        : base(message)
    {
        ErrorCode = errorCode;
        ServiceName = serviceName;
        IsTransient = isTransient;
        Metadata = metadata;
    }

    /// <summary>
    /// Creates an exception for Azure service failures.
    /// </summary>
    public static InfrastructureException AzureService(
        string serviceName,
        string message,
        bool isTransient = true,
        Exception? innerException = null)
    {
        return new InfrastructureException(
            message,
            "AZURE_SERVICE_ERROR",
            serviceName,
            isTransient,
            new Dictionary<string, object> { ["InnerException"] = innerException?.Message ?? "None" });
    }

    /// <summary>
    /// Creates an exception for database failures.
    /// </summary>
    public static InfrastructureException Database(
        string message,
        bool isTransient = true,
        Exception? innerException = null)
    {
        return innerException != null
            ? new InfrastructureException(message, "DATABASE_ERROR", innerException, isTransient)
            : new InfrastructureException(message, "DATABASE_ERROR", isTransient);
    }

    /// <summary>
    /// Creates an exception for network/connectivity failures.
    /// </summary>
    public static InfrastructureException Network(
        string message,
        Exception? innerException = null)
    {
        return innerException != null
            ? new InfrastructureException(message, "NETWORK_ERROR", innerException, isTransient: true)
            : new InfrastructureException(message, "NETWORK_ERROR", isTransient: true);
    }

    /// <summary>
    /// Creates an exception for timeout failures.
    /// </summary>
    public static InfrastructureException Timeout(string message)
    {
        return new InfrastructureException(message, "TIMEOUT_ERROR", isTransient: true);
    }

    /// <summary>
    /// Adds metadata to the exception.
    /// </summary>
    public InfrastructureException WithMetadata(string key, object value)
    {
        var metadata = Metadata != null
            ? new Dictionary<string, object>(Metadata)
            : new Dictionary<string, object>();

        metadata[key] = value;

        return new InfrastructureException(
            Message,
            ErrorCode,
            ServiceName ?? "Unknown",
            IsTransient,
            metadata);
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        var details = new List<string>();

        if (!string.IsNullOrEmpty(ServiceName))
        {
            details.Add($"Service: {ServiceName}");
        }

        details.Add($"IsTransient: {IsTransient}");

        if (Metadata != null)
        {
            details.Add($"Metadata: {string.Join(", ", Metadata.Select(kv => $"{kv.Key}={kv.Value}"))}");
        }

        return $"{baseString}\n{string.Join("\n", details)}";
    }
}
