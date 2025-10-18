namespace IoTTelemetry.Shared.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when input validation fails.
/// Represents errors related to invalid user input or API requests.
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Dictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a validation exception for a single field error.
    /// </summary>
    public static ValidationException ForField(string fieldName, string errorMessage)
    {
        return new ValidationException(
            errorMessage,
            new Dictionary<string, string[]>
            {
                [fieldName] = new[] { errorMessage }
            });
    }

    /// <summary>
    /// Creates a validation exception for multiple field errors.
    /// </summary>
    public static ValidationException ForFields(Dictionary<string, string[]> errors)
    {
        return new ValidationException(errors);
    }

    /// <summary>
    /// Adds a field error to the exception.
    /// </summary>
    public ValidationException WithError(string fieldName, string errorMessage)
    {
        var errors = new Dictionary<string, string[]>(Errors);

        if (errors.TryGetValue(fieldName, out var existingMessages))
        {
            var existingErrors = existingMessages.ToList();
            existingErrors.Add(errorMessage);
            errors[fieldName] = existingErrors.ToArray();
        }
        else
        {
            errors[fieldName] = new[] { errorMessage };
        }

        return new ValidationException(Message, errors);
    }

    /// <summary>
    /// Checks if there are validation errors.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>
    /// Gets all error messages as a flat list.
    /// </summary>
    public IEnumerable<string> GetAllErrors()
    {
        return Errors.Values.SelectMany(e => e);
    }

    public override string ToString()
    {
        var baseString = base.ToString();

        if (!HasErrors)
        {
            return baseString;
        }

        var errorDetails = string.Join("\n",
            Errors.Select(kvp => $"  {kvp.Key}: {string.Join(", ", kvp.Value)}"));

        return $"{baseString}\nValidation Errors:\n{errorDetails}";
    }
}
