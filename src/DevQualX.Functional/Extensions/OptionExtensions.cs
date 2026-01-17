namespace DevQualX.Functional.Extensions;

/// <summary>
/// Extension methods for working with Option types.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Converts a nullable reference type to an option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>Some if the value is not null, None otherwise.</returns>
    public static Option<T> ToOption<T>(this T? value) where T : class =>
        value is not null ? Option.Some(value) : Option.None<T>();

    /// <summary>
    /// Converts a nullable value type to an option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>Some if the value has a value, None otherwise.</returns>
    public static Option<T> ToOption<T>(this T? value) where T : struct =>
        value.HasValue ? Option.Some(value.Value) : Option.None<T>();

    /// <summary>
    /// Returns the first element of a sequence as an option, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <returns>Some with the first element, or None if the sequence is empty.</returns>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            return Option.Some(item);
        }
        return Option.None<T>();
    }

    /// <summary>
    /// Returns the first element of a sequence that satisfies a condition as an option, or None if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <returns>Some with the first matching element, or None if no element matches.</returns>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                return Option.Some(item);
            }
        }
        return Option.None<T>();
    }

    /// <summary>
    /// Returns the single element of a sequence as an option, or None if the sequence is empty or contains more than one element.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <returns>Some with the single element, or None if the sequence is empty or contains multiple elements.</returns>
    public static Option<T> SingleOrNone<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        
        if (!enumerator.MoveNext())
        {
            return Option.None<T>();
        }

        var first = enumerator.Current;
        
        if (enumerator.MoveNext())
        {
            return Option.None<T>();
        }

        return Option.Some(first);
    }

    /// <summary>
    /// Returns the single element of a sequence that satisfies a condition as an option, or None if no such element exists or multiple elements match.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <returns>Some with the single matching element, or None if no element matches or multiple elements match.</returns>
    public static Option<T> SingleOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Where(predicate).SingleOrNone();

    /// <summary>
    /// Returns the last element of a sequence as an option, or None if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <returns>Some with the last element, or None if the sequence is empty.</returns>
    public static Option<T> LastOrNone<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        
        if (!enumerator.MoveNext())
        {
            return Option.None<T>();
        }

        var last = enumerator.Current;
        
        while (enumerator.MoveNext())
        {
            last = enumerator.Current;
        }

        return Option.Some(last);
    }

    /// <summary>
    /// Returns the last element of a sequence that satisfies a condition as an option, or None if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The sequence.</param>
    /// <param name="predicate">The condition to test.</param>
    /// <returns>Some with the last matching element, or None if no element matches.</returns>
    public static Option<T> LastOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate) =>
        source.Where(predicate).LastOrNone();

    /// <summary>
    /// Tries to get a value from a dictionary as an option.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>Some with the value if the key exists, None otherwise.</returns>
    public static Option<TValue> TryGetValueAsOption<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key) where TKey : notnull =>
        dictionary.TryGetValue(key, out var value) ? Option.Some(value) : Option.None<TValue>();

    /// <summary>
    /// Tries to get a value from a read-only dictionary as an option.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>Some with the value if the key exists, None otherwise.</returns>
    public static Option<TValue> TryGetValueAsOption<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dictionary,
        TKey key) where TKey : notnull =>
        dictionary.TryGetValue(key, out var value) ? Option.Some(value) : Option.None<TValue>();

    /// <summary>
    /// Flattens a nested option (Option of Option) into a single option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="option">The nested option.</param>
    /// <returns>A flattened option.</returns>
    public static Option<T> Flatten<T>(this Option<Option<T>> option) =>
        option.Bind(inner => inner);

    /// <summary>
    /// Filters out None values from a sequence of options and unwraps the Some values.
    /// This is useful for LINQ operations to automatically filter and unwrap in one step.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="source">The sequence of options.</param>
    /// <returns>A sequence containing only the unwrapped Some values.</returns>
    /// <example>
    /// <code>
    /// var results = items
    ///     .Select(item => item.ToOption())
    ///     .Choose(); // Filters out None and unwraps Some values
    /// </code>
    /// </example>
    public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> source)
    {
        foreach (var option in source)
        {
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// Applies a function that returns an Option to each element and filters out None values.
    /// This is equivalent to SelectMany followed by Choose, but more efficient.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements.</typeparam>
    /// <typeparam name="TResult">The type of the result values.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">Function that returns an Option for each element.</param>
    /// <returns>A sequence containing only the unwrapped Some values.</returns>
    /// <example>
    /// <code>
    /// var results = items.ChooseMap(item => FindById(item.Id)); // Only returns found items
    /// </code>
    /// </example>
    public static IEnumerable<TResult> ChooseMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, Option<TResult>> selector)
    {
        foreach (var item in source)
        {
            var option = selector(item);
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// Applies an async function that returns an Option to each element and filters out None values.
    /// </summary>
    /// <typeparam name="TSource">The type of the source elements.</typeparam>
    /// <typeparam name="TResult">The type of the result values.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">Async function that returns an Option for each element.</param>
    /// <returns>A task containing a sequence of only the unwrapped Some values.</returns>
    public static async Task<IEnumerable<TResult>> ChooseMapAsync<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<Option<TResult>>> selector)
    {
        var results = new List<TResult>();
        foreach (var item in source)
        {
            var option = await selector(item);
            if (option.IsSome)
            {
                foreach (var value in option)
                {
                    results.Add(value);
                }
            }
        }
        return results;
    }
}
