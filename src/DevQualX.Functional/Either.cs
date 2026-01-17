namespace DevQualX.Functional;

/// <summary>
/// Represents a value that can be one of two types (left or right).
/// Unlike Result, both cases represent valid states (not success/failure).
/// Commonly used for branching logic where both paths are equally valid.
/// </summary>
/// <typeparam name="TLeft">The type of the left value.</typeparam>
/// <typeparam name="TRight">The type of the right value.</typeparam>
public abstract record Either<TLeft, TRight>
{
    /// <summary>
    /// Gets a value indicating whether this either contains a left value.
    /// </summary>
    public abstract bool IsLeft { get; }

    /// <summary>
    /// Gets a value indicating whether this either contains a right value.
    /// </summary>
    public bool IsRight => !IsLeft;

    /// <summary>
    /// Matches the either to one of two functions based on which value it contains.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="left">Function to call if the either contains a left value.</param>
    /// <param name="right">Function to call if the either contains a right value.</param>
    /// <returns>The result of the matching function.</returns>
    public abstract TResult Match<TResult>(Func<TLeft, TResult> left, Func<TRight, TResult> right);

    /// <summary>
    /// Asynchronously matches the either to one of two functions based on which value it contains.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="left">Async function to call if the either contains a left value.</param>
    /// <param name="right">Async function to call if the either contains a right value.</param>
    /// <returns>A task containing the result of the matching function.</returns>
    public abstract Task<TResult> MatchAsync<TResult>(Func<TLeft, Task<TResult>> left, Func<TRight, Task<TResult>> right);

    /// <summary>
    /// Transforms the left value using the provided function.
    /// </summary>
    /// <typeparam name="TLeftResult">The left result type.</typeparam>
    /// <param name="mapper">Function to transform the left value.</param>
    /// <returns>An either with the transformed left value, or the original right value.</returns>
    public abstract Either<TLeftResult, TRight> MapLeft<TLeftResult>(Func<TLeft, TLeftResult> mapper);

    /// <summary>
    /// Transforms the right value using the provided function.
    /// </summary>
    /// <typeparam name="TRightResult">The right result type.</typeparam>
    /// <param name="mapper">Function to transform the right value.</param>
    /// <returns>An either with the transformed right value, or the original left value.</returns>
    public abstract Either<TLeft, TRightResult> MapRight<TRightResult>(Func<TRight, TRightResult> mapper);

    /// <summary>
    /// Asynchronously transforms the left value using the provided function.
    /// </summary>
    /// <typeparam name="TLeftResult">The left result type.</typeparam>
    /// <param name="mapper">Async function to transform the left value.</param>
    /// <returns>A task containing an either with the transformed left value, or the original right value.</returns>
    public abstract Task<Either<TLeftResult, TRight>> MapLeftAsync<TLeftResult>(Func<TLeft, Task<TLeftResult>> mapper);

    /// <summary>
    /// Asynchronously transforms the right value using the provided function.
    /// </summary>
    /// <typeparam name="TRightResult">The right result type.</typeparam>
    /// <param name="mapper">Async function to transform the right value.</param>
    /// <returns>A task containing an either with the transformed right value, or the original left value.</returns>
    public abstract Task<Either<TLeft, TRightResult>> MapRightAsync<TRightResult>(Func<TRight, Task<TRightResult>> mapper);

    /// <summary>
    /// Transforms both left and right values using the provided functions.
    /// </summary>
    /// <typeparam name="TLeftResult">The left result type.</typeparam>
    /// <typeparam name="TRightResult">The right result type.</typeparam>
    /// <param name="leftMapper">Function to transform the left value.</param>
    /// <param name="rightMapper">Function to transform the right value.</param>
    /// <returns>An either with either the transformed left or right value.</returns>
    public abstract Either<TLeftResult, TRightResult> Map<TLeftResult, TRightResult>(
        Func<TLeft, TLeftResult> leftMapper,
        Func<TRight, TRightResult> rightMapper);

    /// <summary>
    /// Binds the either to a function that returns another either if it contains a right value.
    /// </summary>
    /// <typeparam name="TRightResult">The right result type.</typeparam>
    /// <param name="binder">Function that returns an either.</param>
    /// <returns>The result of the binder function, or the original left value.</returns>
    public abstract Either<TLeft, TRightResult> Bind<TRightResult>(Func<TRight, Either<TLeft, TRightResult>> binder);

    /// <summary>
    /// Asynchronously binds the either to a function that returns another either if it contains a right value.
    /// </summary>
    /// <typeparam name="TRightResult">The right result type.</typeparam>
    /// <param name="binder">Async function that returns an either.</param>
    /// <returns>A task containing the result of the binder function, or the original left value.</returns>
    public abstract Task<Either<TLeft, TRightResult>> BindAsync<TRightResult>(Func<TRight, Task<Either<TLeft, TRightResult>>> binder);

    /// <summary>
    /// Executes an action if the either contains a left value.
    /// </summary>
    /// <param name="action">Action to execute with the left value.</param>
    /// <returns>This either for chaining.</returns>
    public abstract Either<TLeft, TRight> IfLeft(Action<TLeft> action);

    /// <summary>
    /// Executes an action if the either contains a right value.
    /// </summary>
    /// <param name="action">Action to execute with the right value.</param>
    /// <returns>This either for chaining.</returns>
    public abstract Either<TLeft, TRight> IfRight(Action<TRight> action);

    /// <summary>
    /// Swaps left and right values.
    /// </summary>
    /// <returns>An either with left and right swapped.</returns>
    public abstract Either<TRight, TLeft> Swap();

    /// <summary>
    /// Implicitly converts a left value to Either.
    /// </summary>
    public static implicit operator Either<TLeft, TRight>(TLeft left) => new Left<TLeft, TRight>(left);

    /// <summary>
    /// Implicitly converts a right value to Either.
    /// </summary>
    public static implicit operator Either<TLeft, TRight>(TRight right) => new Right<TLeft, TRight>(right);

    /// <summary>
    /// Maps the either using a selector (for LINQ support on right values).
    /// </summary>
    public Either<TLeft, TRightResult> Select<TRightResult>(Func<TRight, TRightResult> selector) => MapRight(selector);

    /// <summary>
    /// Binds the either using a selector (for LINQ query syntax support on right values).
    /// </summary>
    public Either<TLeft, TRightResult> SelectMany<TRightResult>(Func<TRight, Either<TLeft, TRightResult>> selector) => Bind(selector);

    /// <summary>
    /// Binds and projects the either (for LINQ query syntax support on right values).
    /// </summary>
    public Either<TLeft, TRightResult> SelectMany<TRightIntermediate, TRightResult>(
        Func<TRight, Either<TLeft, TRightIntermediate>> selector,
        Func<TRight, TRightIntermediate, TRightResult> projector) =>
        Bind(right => selector(right).MapRight(intermediate => projector(right, intermediate)));
}

/// <summary>
/// Represents an either containing a left value.
/// </summary>
/// <typeparam name="TLeft">The type of the left value.</typeparam>
/// <typeparam name="TRight">The type parameter for the right.</typeparam>
public sealed record Left<TLeft, TRight>(TLeft Value) : Either<TLeft, TRight>
{
    public override bool IsLeft => true;

    public override TResult Match<TResult>(Func<TLeft, TResult> left, Func<TRight, TResult> right) =>
        left(Value);

    public override async Task<TResult> MatchAsync<TResult>(Func<TLeft, Task<TResult>> left, Func<TRight, Task<TResult>> right) =>
        await left(Value);

    public override Either<TLeftResult, TRight> MapLeft<TLeftResult>(Func<TLeft, TLeftResult> mapper) =>
        new Left<TLeftResult, TRight>(mapper(Value));

    public override Either<TLeft, TRightResult> MapRight<TRightResult>(Func<TRight, TRightResult> mapper) =>
        new Left<TLeft, TRightResult>(Value);

    public override async Task<Either<TLeftResult, TRight>> MapLeftAsync<TLeftResult>(Func<TLeft, Task<TLeftResult>> mapper) =>
        new Left<TLeftResult, TRight>(await mapper(Value));

    public override Task<Either<TLeft, TRightResult>> MapRightAsync<TRightResult>(Func<TRight, Task<TRightResult>> mapper) =>
        Task.FromResult<Either<TLeft, TRightResult>>(new Left<TLeft, TRightResult>(Value));

    public override Either<TLeftResult, TRightResult> Map<TLeftResult, TRightResult>(
        Func<TLeft, TLeftResult> leftMapper,
        Func<TRight, TRightResult> rightMapper) =>
        new Left<TLeftResult, TRightResult>(leftMapper(Value));

    public override Either<TLeft, TRightResult> Bind<TRightResult>(Func<TRight, Either<TLeft, TRightResult>> binder) =>
        new Left<TLeft, TRightResult>(Value);

    public override Task<Either<TLeft, TRightResult>> BindAsync<TRightResult>(Func<TRight, Task<Either<TLeft, TRightResult>>> binder) =>
        Task.FromResult<Either<TLeft, TRightResult>>(new Left<TLeft, TRightResult>(Value));

    public override Either<TLeft, TRight> IfLeft(Action<TLeft> action)
    {
        action(Value);
        return this;
    }

    public override Either<TLeft, TRight> IfRight(Action<TRight> action) => this;

    public override Either<TRight, TLeft> Swap() => new Right<TRight, TLeft>(Value);
}

/// <summary>
/// Represents an either containing a right value.
/// </summary>
/// <typeparam name="TLeft">The type parameter for the left.</typeparam>
/// <typeparam name="TRight">The type of the right value.</typeparam>
public sealed record Right<TLeft, TRight>(TRight Value) : Either<TLeft, TRight>
{
    public override bool IsLeft => false;

    public override TResult Match<TResult>(Func<TLeft, TResult> left, Func<TRight, TResult> right) =>
        right(Value);

    public override async Task<TResult> MatchAsync<TResult>(Func<TLeft, Task<TResult>> left, Func<TRight, Task<TResult>> right) =>
        await right(Value);

    public override Either<TLeftResult, TRight> MapLeft<TLeftResult>(Func<TLeft, TLeftResult> mapper) =>
        new Right<TLeftResult, TRight>(Value);

    public override Either<TLeft, TRightResult> MapRight<TRightResult>(Func<TRight, TRightResult> mapper) =>
        new Right<TLeft, TRightResult>(mapper(Value));

    public override Task<Either<TLeftResult, TRight>> MapLeftAsync<TLeftResult>(Func<TLeft, Task<TLeftResult>> mapper) =>
        Task.FromResult<Either<TLeftResult, TRight>>(new Right<TLeftResult, TRight>(Value));

    public override async Task<Either<TLeft, TRightResult>> MapRightAsync<TRightResult>(Func<TRight, Task<TRightResult>> mapper) =>
        new Right<TLeft, TRightResult>(await mapper(Value));

    public override Either<TLeftResult, TRightResult> Map<TLeftResult, TRightResult>(
        Func<TLeft, TLeftResult> leftMapper,
        Func<TRight, TRightResult> rightMapper) =>
        new Right<TLeftResult, TRightResult>(rightMapper(Value));

    public override Either<TLeft, TRightResult> Bind<TRightResult>(Func<TRight, Either<TLeft, TRightResult>> binder) =>
        binder(Value);

    public override async Task<Either<TLeft, TRightResult>> BindAsync<TRightResult>(Func<TRight, Task<Either<TLeft, TRightResult>>> binder) =>
        await binder(Value);

    public override Either<TLeft, TRight> IfLeft(Action<TLeft> action) => this;

    public override Either<TLeft, TRight> IfRight(Action<TRight> action)
    {
        action(Value);
        return this;
    }

    public override Either<TRight, TLeft> Swap() => new Left<TRight, TLeft>(Value);
}

/// <summary>
/// Provides static factory methods for creating Either instances.
/// </summary>
public static class Either
{
    /// <summary>
    /// Creates an either with a left value.
    /// </summary>
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value) => new Left<TLeft, TRight>(value);

    /// <summary>
    /// Creates an either with a right value.
    /// </summary>
    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value) => new Right<TLeft, TRight>(value);
}
