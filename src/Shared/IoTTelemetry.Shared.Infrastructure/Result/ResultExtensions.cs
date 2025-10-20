namespace IoTTelemetry.Shared.Infrastructure.Result;

/// <summary>
/// Extension methods for Result types to enable functional composition.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure)
        {
            action(result.Error!);
        }

        return result;
    }

    /// <summary>
    /// Executes an async action if the result is successful.
    /// </summary>
    public static async Task<Result<T>> OnSuccessAsync<T>(this Result<T> result, Func<T, Task> action)
    {
        if (result.IsSuccess)
        {
            await action(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Executes an async action if the result is a failure.
    /// </summary>
    public static async Task<Result<T>> OnFailureAsync<T>(this Result<T> result, Func<Error, Task> action)
    {
        if (result.IsFailure)
        {
            await action(result.Error!);
        }

        return result;
    }

    /// <summary>
    /// Ensures a condition is met, otherwise returns a failure.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value!)
            ? result
            : Result<T>.Failure(error);
    }

    /// <summary>
    /// Combines multiple results into one. Returns first failure or success.
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Combines multiple results into one. Returns first failure or success with aggregated values.
    /// </summary>
    public static Result<IEnumerable<T>> Combine<T>(params Result<T>[] results)
    {
        var values = new List<T>();

        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return Result<IEnumerable<T>>.Failure(result.Error!);
            }

            values.Add(result.Value!);
        }

        return Result<IEnumerable<T>>.Success(values);
    }

    /// <summary>
    /// Flattens a nested Result&lt;Result&lt;T&gt;&gt; into Result&lt;T&gt;.
    /// </summary>
    public static Result<T> Flatten<T>(this Result<Result<T>> result)
    {
        return result.IsFailure
            ? Result<T>.Failure(result.Error!)
            : result.Value!;
    }

    /// <summary>
    /// Converts a Result to a Task&lt;Result&gt;.
    /// </summary>
    public static Task<Result<T>> AsTask<T>(this Result<T> result)
    {
        return Task.FromResult(result);
    }

    /// <summary>
    /// Converts a Result to a ValueTask&lt;Result&gt;.
    /// </summary>
    public static ValueTask<Result<T>> AsValueTask<T>(this Result<T> result)
    {
        return ValueTask.FromResult(result);
    }

    /// <summary>
    /// Maps over multiple results at once.
    /// </summary>
    public static Result<TResult> Map<T1, T2, TResult>(
        Result<T1> result1,
        Result<T2> result2,
        Func<T1, T2, TResult> mapper)
    {
        if (result1.IsFailure)
        {
            return Result<TResult>.Failure(result1.Error!);
        }

        if (result2.IsFailure)
        {
            return Result<TResult>.Failure(result2.Error!);
        }

        return Result<TResult>.Success(mapper(result1.Value!, result2.Value!));
    }

    /// <summary>
    /// Filters a result based on a predicate.
    /// </summary>
    public static Result<T> Filter<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
        {
            return result;
        }

        return predicate(result.Value!)
            ? result
            : Result<T>.Failure(error);
    }

    /// <summary>
    /// Taps into the result for side effects without changing it (useful for logging).
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Taps into a failure for side effects (useful for logging errors).
    /// </summary>
    public static Result<T> TapError<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure)
        {
            action(result.Error!);
        }

        return result;
    }
}
