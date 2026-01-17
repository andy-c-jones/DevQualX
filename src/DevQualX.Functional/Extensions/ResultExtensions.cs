namespace DevQualX.Functional.Extensions;

/// <summary>
/// Extension methods for working with Result types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes a function and wraps the result or any exception in a Result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>Success with the value, or Failure with an InternalError if an exception occurs.</returns>
    public static Result<T, Error> ToResult<T>(Func<T> func)
    {
        try
        {
            return Result.Success<T, Error>(func());
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(new InternalError
            {
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object>
                {
                    ["StackTrace"] = ex.StackTrace ?? string.Empty
                }
            });
        }
    }

    /// <summary>
    /// Executes an async function and wraps the result or any exception in a Result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <returns>A task containing Success with the value, or Failure with an InternalError if an exception occurs.</returns>
    public static async Task<Result<T, Error>> ToResultAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return Result.Success<T, Error>(await func());
        }
        catch (Exception ex)
        {
            return Result.Failure<T, Error>(new InternalError
            {
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                Metadata = new Dictionary<string, object>
                {
                    ["StackTrace"] = ex.StackTrace ?? string.Empty
                }
            });
        }
    }

    /// <summary>
    /// Combines multiple results into a single result containing a list of values.
    /// If any result is a failure, returns the first failure.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="results">The results to combine.</param>
    /// <returns>Success with all values, or the first Failure encountered.</returns>
    public static Result<IEnumerable<T>, TError> Combine<T, TError>(this IEnumerable<Result<T, TError>> results)
        where TError : Error
    {
        var values = new List<T>();
        
        foreach (var result in results)
        {
            if (result.IsFailure)
            {
                return result.Match<Result<IEnumerable<T>, TError>>(
                    success: _ => throw new InvalidOperationException("Unexpected success"),
                    failure: error => Result.Failure<IEnumerable<T>, TError>(error)
                );
            }
            
            values.Add(result.Match(
                success: value => value,
                failure: _ => throw new InvalidOperationException("Unexpected failure")
            ));
        }
        
        return Result.Success<IEnumerable<T>, TError>(values);
    }

    /// <summary>
    /// Collects all errors from a sequence of results.
    /// Returns Success with all values if all succeed, or Failure with all collected errors.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="results">The results to process.</param>
    /// <returns>Success with all values, or Failure with aggregated ValidationError.</returns>
    public static Result<IEnumerable<T>, ValidationError> CollectErrors<T>(
        this IEnumerable<Result<T, ValidationError>> results)
    {
        var values = new List<T>();
        var errors = new List<ValidationError>();
        
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                values.Add(result.Match(success: v => v, failure: _ => default!));
            }
            else
            {
                errors.Add(result.Match(success: _ => default!, failure: e => e));
            }
        }
        
        if (errors.Count > 0)
        {
            return Result.Failure<IEnumerable<T>, ValidationError>(new ValidationError
            {
                Message = $"{errors.Count} validation error(s) occurred",
                Errors = errors
                    .SelectMany(e => e.Errors ?? new Dictionary<string, string[]>())
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => g.SelectMany(kvp => kvp.Value).ToArray()
                    )
            });
        }
        
        return Result.Success<IEnumerable<T>, ValidationError>(values);
    }

    /// <summary>
    /// Filters out failures from a sequence of results and unwraps the success values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The sequence of results.</param>
    /// <returns>A sequence containing only the unwrapped success values.</returns>
    public static IEnumerable<T> Choose<T, TError>(this IEnumerable<Result<T, TError>> source)
        where TError : Error
    {
        foreach (var result in source)
        {
            if (result.IsSuccess)
            {
                yield return result.Match(
                    success: value => value,
                    failure: _ => throw new InvalidOperationException("Unexpected failure")
                );
            }
        }
    }

    /// <summary>
    /// Partitions a sequence of results into successes and failures.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="source">The sequence of results.</param>
    /// <returns>A tuple of (successes, failures).</returns>
    public static (IEnumerable<T> Successes, IEnumerable<TError> Failures) Partition<T, TError>(
        this IEnumerable<Result<T, TError>> source)
        where TError : Error
    {
        var successes = new List<T>();
        var failures = new List<TError>();
        
        foreach (var result in source)
        {
            if (result.IsSuccess)
            {
                successes.Add(result.Match(success: v => v, failure: _ => default!));
            }
            else
            {
                failures.Add(result.Match(success: _ => default!, failure: e => e));
            }
        }
        
        return (successes, failures);
    }
}
