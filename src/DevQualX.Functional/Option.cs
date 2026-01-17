using System.Collections;

namespace DevQualX.Functional;

/// <summary>
/// Represents an optional value that may or may not be present.
/// Provides a type-safe alternative to null references.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public abstract record Option<T> : IEnumerable<T>
{
    /// <summary>
    /// Gets a value indicating whether this option contains a value.
    /// </summary>
    public abstract bool IsSome { get; }

    /// <summary>
    /// Gets a value indicating whether this option is empty.
    /// </summary>
    public bool IsNone => !IsSome;

    /// <summary>
    /// Matches the option to one of two functions based on whether it has a value.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="some">Function to call if the option has a value.</param>
    /// <param name="none">Function to call if the option is empty.</param>
    /// <returns>The result of the matching function.</returns>
    public abstract TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none);

    /// <summary>
    /// Asynchronously matches the option to one of two functions based on whether it has a value.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="some">Async function to call if the option has a value.</param>
    /// <param name="none">Async function to call if the option is empty.</param>
    /// <returns>A task containing the result of the matching function.</returns>
    public abstract Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> some, Func<Task<TResult>> none);

    /// <summary>
    /// Transforms the value inside the option using the provided function.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="mapper">Function to transform the value.</param>
    /// <returns>An option containing the transformed value, or None if this option is empty.</returns>
    public abstract Option<TResult> Map<TResult>(Func<T, TResult> mapper);

    /// <summary>
    /// Asynchronously transforms the value inside the option using the provided function.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="mapper">Async function to transform the value.</param>
    /// <returns>A task containing an option with the transformed value, or None if this option is empty.</returns>
    public abstract Task<Option<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper);

    /// <summary>
    /// Binds the option to a function that returns another option (flatMap).
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="binder">Function that returns an option.</param>
    /// <returns>The result of the binder function, or None if this option is empty.</returns>
    public abstract Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder);

    /// <summary>
    /// Asynchronously binds the option to a function that returns another option.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="binder">Async function that returns an option.</param>
    /// <returns>A task containing the result of the binder function, or None if this option is empty.</returns>
    public abstract Task<Option<TResult>> BindAsync<TResult>(Func<T, Task<Option<TResult>>> binder);

    /// <summary>
    /// Filters the option based on a predicate.
    /// </summary>
    /// <param name="predicate">Function to test the value.</param>
    /// <returns>This option if it has a value and the predicate returns true, otherwise None.</returns>
    public abstract Option<T> Filter(Func<T, bool> predicate);

    /// <summary>
    /// Returns the value if present, otherwise returns the provided default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the option is empty.</param>
    /// <returns>The option's value or the default value.</returns>
    public abstract T GetValueOrDefault(T defaultValue);

    /// <summary>
    /// Returns the value if present, otherwise calls the provided factory function.
    /// </summary>
    /// <param name="defaultFactory">Function to produce a default value.</param>
    /// <returns>The option's value or the result of the factory function.</returns>
    public abstract T GetValueOrDefault(Func<T> defaultFactory);

    /// <summary>
    /// Returns this option if it has a value, otherwise returns the alternative option.
    /// </summary>
    /// <param name="alternative">The alternative option to return if this is None.</param>
    /// <returns>This option or the alternative.</returns>
    public abstract Option<T> OrElse(Option<T> alternative);

    /// <summary>
    /// Returns this option if it has a value, otherwise calls the provided factory function.
    /// </summary>
    /// <param name="alternativeFactory">Function to produce an alternative option.</param>
    /// <returns>This option or the result of the factory function.</returns>
    public abstract Option<T> OrElse(Func<Option<T>> alternativeFactory);

    /// <summary>
    /// Executes an action if the option has a value.
    /// </summary>
    /// <param name="action">Action to execute with the value.</param>
    /// <returns>This option for chaining.</returns>
    public abstract Option<T> IfSome(Action<T> action);

    /// <summary>
    /// Executes an action if the option is empty.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <returns>This option for chaining.</returns>
    public abstract Option<T> IfNone(Action action);

    /// <summary>
    /// Returns an enumerator that yields the value if present (for LINQ support).
    /// </summary>
    public abstract IEnumerator<T> GetEnumerator();

    /// <summary>
    /// Returns an enumerator that yields the value if present.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Implicitly converts a value to Some.
    /// </summary>
    public static implicit operator Option<T>(T value) => new Some<T>(value);

    /// <summary>
    /// Maps the option using a selector (for LINQ support).
    /// </summary>
    public Option<TResult> Select<TResult>(Func<T, TResult> selector) => Map(selector);

    /// <summary>
    /// Binds the option using a selector (for LINQ query syntax support).
    /// </summary>
    public Option<TResult> SelectMany<TResult>(Func<T, Option<TResult>> selector) => Bind(selector);

    /// <summary>
    /// Binds and projects the option (for LINQ query syntax support).
    /// </summary>
    public Option<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Option<TIntermediate>> selector,
        Func<T, TIntermediate, TResult> projector) =>
        Bind(value => selector(value).Map(intermediate => projector(value, intermediate)));

    /// <summary>
    /// Filters the option using a predicate (for LINQ support).
    /// </summary>
    public Option<T> Where(Func<T, bool> predicate) => Filter(predicate);
}

/// <summary>
/// Represents an option with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public sealed record Some<T>(T Value) : Option<T>
{
    public override bool IsSome => true;

    public override TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) =>
        some(Value);

    public override async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> some, Func<Task<TResult>> none) =>
        await some(Value);

    public override Option<TResult> Map<TResult>(Func<T, TResult> mapper) =>
        new Some<TResult>(mapper(Value));

    public override async Task<Option<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper) =>
        new Some<TResult>(await mapper(Value));

    public override Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) =>
        binder(Value);

    public override async Task<Option<TResult>> BindAsync<TResult>(Func<T, Task<Option<TResult>>> binder) =>
        await binder(Value);

    public override Option<T> Filter(Func<T, bool> predicate) =>
        predicate(Value) ? this : new None<T>();

    public override T GetValueOrDefault(T defaultValue) => Value;

    public override T GetValueOrDefault(Func<T> defaultFactory) => Value;

    public override Option<T> OrElse(Option<T> alternative) => this;

    public override Option<T> OrElse(Func<Option<T>> alternativeFactory) => this;

    public override Option<T> IfSome(Action<T> action)
    {
        action(Value);
        return this;
    }

    public override Option<T> IfNone(Action action) => this;

    public override IEnumerator<T> GetEnumerator()
    {
        yield return Value;
    }
}

/// <summary>
/// Represents an empty option with no value.
/// </summary>
/// <typeparam name="T">The type parameter.</typeparam>
public sealed record None<T> : Option<T>
{
    public override bool IsSome => false;

    public override TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) =>
        none();

    public override async Task<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> some, Func<Task<TResult>> none) =>
        await none();

    public override Option<TResult> Map<TResult>(Func<T, TResult> mapper) =>
        new None<TResult>();

    public override Task<Option<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> mapper) =>
        Task.FromResult<Option<TResult>>(new None<TResult>());

    public override Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) =>
        new None<TResult>();

    public override Task<Option<TResult>> BindAsync<TResult>(Func<T, Task<Option<TResult>>> binder) =>
        Task.FromResult<Option<TResult>>(new None<TResult>());

    public override Option<T> Filter(Func<T, bool> predicate) => this;

    public override T GetValueOrDefault(T defaultValue) => defaultValue;

    public override T GetValueOrDefault(Func<T> defaultFactory) => defaultFactory();

    public override Option<T> OrElse(Option<T> alternative) => alternative;

    public override Option<T> OrElse(Func<Option<T>> alternativeFactory) => alternativeFactory();

    public override Option<T> IfSome(Action<T> action) => this;

    public override Option<T> IfNone(Action action)
    {
        action();
        return this;
    }

    public override IEnumerator<T> GetEnumerator()
    {
        yield break;
    }
}

/// <summary>
/// Provides static factory methods for creating Option instances.
/// </summary>
public static class Option
{
    /// <summary>
    /// Creates an option with a value.
    /// </summary>
    public static Option<T> Some<T>(T value) => new Some<T>(value);

    /// <summary>
    /// Creates an empty option.
    /// </summary>
    public static Option<T> None<T>() => new None<T>();

    /// <summary>
    /// Converts a nullable value to an option.
    /// </summary>
    public static Option<T> From<T>(T? value) where T : class =>
        value is not null ? new Some<T>(value) : new None<T>();

    /// <summary>
    /// Converts a nullable value type to an option.
    /// </summary>
    public static Option<T> From<T>(T? value) where T : struct =>
        value.HasValue ? new Some<T>(value.Value) : new None<T>();
}
