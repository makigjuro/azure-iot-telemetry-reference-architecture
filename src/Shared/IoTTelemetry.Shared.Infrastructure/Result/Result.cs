namespace IoTTelemetry.Shared.Infrastructure.Result;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Generic version for operations that return a value.
/// </summary>
#pragma warning disable CA1000 // Static factory methods are standard for Result pattern
public sealed class Result<T>
#pragma warning restore CA1000
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T> Success(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "Success value cannot be null.");
        }

        return new Result<T>(value);
    }

    /// <summary>
    /// Creates a failed result with an error.
    /// </summary>
    public static Result<T> Failure(Error error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error), "Error cannot be null.");
        }

        return new Result<T>(error);
    }

    /// <summary>
    /// Executes one of two functions depending on success/failure.
    /// </summary>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }

    /// <summary>
    /// Executes one of two actions depending on success/failure.
    /// </summary>
    public void Match(
        Action<T> onSuccess,
        Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(Value!);
        }
        else
        {
            onFailure(Error!);
        }
    }

    /// <summary>
    /// Transforms the value if successful, otherwise returns the error.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapper(Value!))
            : Result<TNew>.Failure(Error!);
    }

    /// <summary>
    /// Chains another operation if successful (railway-oriented programming).
    /// </summary>
    public Result<TNew> Then<TNew>(Func<T, Result<TNew>> next)
    {
        return IsSuccess ? next(Value!) : Result<TNew>.Failure(Error!);
    }

    /// <summary>
    /// Chains another async operation if successful.
    /// </summary>
    public async Task<Result<TNew>> ThenAsync<TNew>(Func<T, Task<Result<TNew>>> next)
    {
        return IsSuccess ? await next(Value!) : Result<TNew>.Failure(Error!);
    }

    /// <summary>
    /// Returns the value if successful, otherwise throws an exception.
    /// </summary>
    public T Unwrap()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException($"Cannot unwrap a failed result: {Error}");
        }

        return Value!;
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value.
    /// </summary>
    public T? UnwrapOr(T defaultValue)
    {
        return IsSuccess ? Value : defaultValue;
    }

    /// <summary>
    /// Returns the value if successful, otherwise computes the default value.
    /// </summary>
    public T UnwrapOrElse(Func<Error, T> defaultValueProvider)
    {
        return IsSuccess ? Value! : defaultValueProvider(Error!);
    }

    /// <summary>
    /// Implicit conversion from value to successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from error to failed result.
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Non-generic result for operations that don't return a value.
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with an error.
    /// </summary>
    public static Result Failure(Error error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error), "Error cannot be null.");
        }

        return new Result(false, error);
    }

    /// <summary>
    /// Executes one of two functions depending on success/failure.
    /// </summary>
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error!);
    }

    /// <summary>
    /// Executes one of two actions depending on success/failure.
    /// </summary>
    public void Match(
        Action onSuccess,
        Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess();
        }
        else
        {
            onFailure(Error!);
        }
    }

    /// <summary>
    /// Chains another operation if successful.
    /// </summary>
    public Result Then(Func<Result> next)
    {
        return IsSuccess ? next() : this;
    }

    /// <summary>
    /// Chains another async operation if successful.
    /// </summary>
    public async Task<Result> ThenAsync(Func<Task<Result>> next)
    {
        return IsSuccess ? await next() : this;
    }

    /// <summary>
    /// Converts to Result<T> by providing a value.
    /// </summary>
    public Result<T> WithValue<T>(T value)
    {
        return IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(Error!);
    }

    /// <summary>
    /// Throws an exception if the result is a failure.
    /// </summary>
    public void ThrowIfFailure()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException($"Operation failed: {Error}");
        }
    }

    /// <summary>
    /// Implicit conversion from error to failed result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);
}
