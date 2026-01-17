namespace DevQualX.Functional;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// Provides a type-safe alternative to throwing exceptions for known error conditions.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <typeparam name="TError">The type of the error, must inherit from Error.</typeparam>
public abstract record Result<T, TError> where TError : Error
{
    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// </summary>
    public abstract bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="success">Function to call if the result is successful.</param>
    /// <param name="failure">Function to call if the result is a failure.</param>
    /// <returns>The result of the matching function.</returns>
    public abstract TResult Match<TResult>(Func<T, TResult> success, Func<TError, TResult> failure);

    /// <summary>
    /// Asynchronously matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="success">Async function to call if the result is successful.</param>
    /// <param name="failure">Async function to call if the result is a failure.</param>
    /// <returns>A task containing the result of the matching function.</returns>
    public abstract Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> success, Func<TError, Task<TResult>> failure);

    /// <summary>
    /// Transforms the success value using the provided function.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="mapper">Function to transform the success value.</param>
    /// <returns>A result with the transformed value, or the original failure.</returns>
    public abstract Result<TResult, TError> Map<TResult>(Func<T, TResult> mapper);

    /// <summary>
    /// Asynchronously transforms the success value using the provided function.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="mapper">Async function to transform the success value.</param>
    /// <returns>A task containing a result with the transformed value, or the original failure.</returns>
    public abstract Task<Result<TResult, TError>> MapAsync<TResult>(Func<T, Task<TResult>> mapper);

    /// <summary>
    /// Transforms the error value using the provided function.
    /// </summary>
    /// <typeparam name="TErrorResult">The error result type.</typeparam>
    /// <param name="mapper">Function to transform the error value.</param>
    /// <returns>A result with the transformed error, or the original success value.</returns>
    public abstract Result<T, TErrorResult> MapError<TErrorResult>(Func<TError, TErrorResult> mapper) where TErrorResult : Error;

    /// <summary>
    /// Binds the result to a function that returns another result (flatMap).
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="binder">Function that returns a result.</param>
    /// <returns>The result of the binder function, or the original failure.</returns>
    public abstract Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> binder);

    /// <summary>
    /// Asynchronously binds the result to a function that returns another result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="binder">Async function that returns a result.</param>
    /// <returns>A task containing the result of the binder function, or the original failure.</returns>
    public abstract Task<Result<TResult, TError>> BindAsync<TResult>(Func<T, Task<Result<TResult, TError>>> binder);

    /// <summary>
    /// Executes an action if the result is successful (side effect).
    /// </summary>
    /// <param name="action">Action to execute with the success value.</param>
    /// <returns>This result for chaining.</returns>
    public abstract Result<T, TError> Tap(Action<T> action);

    /// <summary>
    /// Executes an action if the result is a failure (side effect).
    /// </summary>
    /// <param name="action">Action to execute with the error.</param>
    /// <returns>This result for chaining.</returns>
    public abstract Result<T, TError> TapError(Action<TError> action);

    /// <summary>
    /// Returns the success value if present, otherwise returns the provided default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the result is a failure.</param>
    /// <returns>The success value or the default value.</returns>
    public abstract T GetValueOrDefault(T defaultValue);

    /// <summary>
    /// Returns the success value if present, otherwise calls the provided factory function.
    /// </summary>
    /// <param name="defaultFactory">Function to produce a default value.</param>
    /// <returns>The success value or the result of the factory function.</returns>
    public abstract T GetValueOrDefault(Func<T> defaultFactory);

    /// <summary>
    /// Returns the success value if present, otherwise throws an exception.
    /// Use this as an escape hatch - prefer Match or GetValueOrDefault.
    /// </summary>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
    public abstract T GetValueOrThrow();

    /// <summary>
    /// Returns this result if successful, otherwise returns the alternative result.
    /// </summary>
    /// <param name="alternative">The alternative result to return if this is a failure.</param>
    /// <returns>This result or the alternative.</returns>
    public abstract Result<T, TError> OrElse(Result<T, TError> alternative);

    /// <summary>
    /// Returns this result if successful, otherwise calls the provided factory function.
    /// </summary>
    /// <param name="alternativeFactory">Function to produce an alternative result.</param>
    /// <returns>This result or the result of the factory function.</returns>
    public abstract Result<T, TError> OrElse(Func<Result<T, TError>> alternativeFactory);

    /// <summary>
    /// Converts the result to an option, discarding any error information.
    /// </summary>
    /// <returns>Some with the value if successful, None if failure.</returns>
    public abstract Option<T> AsOption();

    /// <summary>
    /// Implicitly converts a value to Success.
    /// </summary>
    public static implicit operator Result<T, TError>(T value) => new Success<T, TError>(value);

    /// <summary>
    /// Implicitly converts an error to Failure.
    /// </summary>
    public static implicit operator Result<T, TError>(TError error) => new Failure<T, TError>(error);

    /// <summary>
    /// Maps the result using a selector (for LINQ support).
    /// </summary>
    public Result<TResult, TError> Select<TResult>(Func<T, TResult> selector) => Map(selector);

    /// <summary>
    /// Binds the result using a selector (for LINQ query syntax support).
    /// </summary>
    public Result<TResult, TError> SelectMany<TResult>(Func<T, Result<TResult, TError>> selector) => Bind(selector);

    /// <summary>
    /// Binds and projects the result (for LINQ query syntax support).
    /// </summary>
    public Result<TResult, TError> SelectMany<TIntermediate, TResult>(
        Func<T, Result<TIntermediate, TError>> selector,
        Func<T, TIntermediate, TResult> projector) =>
        Bind(value => selector(value).Map(intermediate => projector(value, intermediate)));
}

/// <summary>
/// Represents a successful result with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <typeparam name="TError">The error type.</typeparam>
public sealed record Success<T, TError>(T Value) : Result<T, TError> where TError : Error
{
    public override bool IsSuccess => true;

    public override TResult Match<TResult>(Func<T, TResult> success, Func<TError, TResult> failure) =>
        success(Value);

    public override async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> success, Func<TError, Task<TResult>> failure) =>
        await success(Value);

    public override Result<TResult, TError> Map<TResult>(Func<T, TResult> mapper) =>
        new Success<TResult, TError>(mapper(Value));

    public override async Task<Result<TResult, TError>> MapAsync<TResult>(Func<T, Task<TResult>> mapper) =>
        new Success<TResult, TError>(await mapper(Value));

    public override Result<T, TErrorResult> MapError<TErrorResult>(Func<TError, TErrorResult> mapper) =>
        new Success<T, TErrorResult>(Value);

    public override Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> binder) =>
        binder(Value);

    public override async Task<Result<TResult, TError>> BindAsync<TResult>(Func<T, Task<Result<TResult, TError>>> binder) =>
        await binder(Value);

    public override Result<T, TError> Tap(Action<T> action)
    {
        action(Value);
        return this;
    }

    public override Result<T, TError> TapError(Action<TError> action) => this;

    public override T GetValueOrDefault(T defaultValue) => Value;

    public override T GetValueOrDefault(Func<T> defaultFactory) => Value;

    public override T GetValueOrThrow() => Value;

    public override Result<T, TError> OrElse(Result<T, TError> alternative) => this;

    public override Result<T, TError> OrElse(Func<Result<T, TError>> alternativeFactory) => this;

    public override Option<T> AsOption() => new Some<T>(Value);
}

/// <summary>
/// Represents a failed result with an error.
/// </summary>
/// <typeparam name="T">The type parameter.</typeparam>
/// <typeparam name="TError">The error type.</typeparam>
public sealed record Failure<T, TError>(TError Error) : Result<T, TError> where TError : Error
{
    public override bool IsSuccess => false;

    public override TResult Match<TResult>(Func<T, TResult> success, Func<TError, TResult> failure) =>
        failure(Error);

    public override async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> success, Func<TError, Task<TResult>> failure) =>
        await failure(Error);

    public override Result<TResult, TError> Map<TResult>(Func<T, TResult> mapper) =>
        new Failure<TResult, TError>(Error);

    public override Task<Result<TResult, TError>> MapAsync<TResult>(Func<T, Task<TResult>> mapper) =>
        Task.FromResult<Result<TResult, TError>>(new Failure<TResult, TError>(Error));

    public override Result<T, TErrorResult> MapError<TErrorResult>(Func<TError, TErrorResult> mapper) =>
        new Failure<T, TErrorResult>(mapper(Error));

    public override Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> binder) =>
        new Failure<TResult, TError>(Error);

    public override Task<Result<TResult, TError>> BindAsync<TResult>(Func<T, Task<Result<TResult, TError>>> binder) =>
        Task.FromResult<Result<TResult, TError>>(new Failure<TResult, TError>(Error));

    public override Result<T, TError> Tap(Action<T> action) => this;

    public override Result<T, TError> TapError(Action<TError> action)
    {
        action(Error);
        return this;
    }

    public override T GetValueOrDefault(T defaultValue) => defaultValue;

    public override T GetValueOrDefault(Func<T> defaultFactory) => defaultFactory();

    public override T GetValueOrThrow() =>
        throw new InvalidOperationException($"Cannot get value from a failed result. Error: {Error.Message}");

    public override Result<T, TError> OrElse(Result<T, TError> alternative) => alternative;

    public override Result<T, TError> OrElse(Func<Result<T, TError>> alternativeFactory) => alternativeFactory();

    public override Option<T> AsOption() => new None<T>();
}

/// <summary>
/// Provides static factory methods for creating Result instances.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T, TError> Success<T, TError>(T value) where TError : Error =>
        new Success<T, TError>(value);

    /// <summary>
    /// Creates a failed result with an error.
    /// </summary>
    public static Result<T, TError> Failure<T, TError>(TError error) where TError : Error =>
        new Failure<T, TError>(error);
}
