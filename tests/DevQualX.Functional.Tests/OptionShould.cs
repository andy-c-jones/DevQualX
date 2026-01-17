using DevQualX.Functional;
using DevQualX.Functional.Extensions;

namespace DevQualX.Functional.Tests;

/// <summary>
/// Tests for Option<T> type and its operations.
/// </summary>
public class OptionShould
{
    [Test]
    public async Task Create_some_with_value()
    {
        // Arrange & Act
        var option = Option.Some(42);
        
        // Assert
        await Assert.That(option.IsSome).IsTrue();
        await Assert.That(option.IsNone).IsFalse();
    }

    [Test]
    public async Task Create_none_without_value()
    {
        // Arrange & Act
        var option = Option.None<int>();
        
        // Assert
        await Assert.That(option.IsSome).IsFalse();
        await Assert.That(option.IsNone).IsTrue();
    }

    [Test]
    public async Task Match_some_calls_some_function()
    {
        // Arrange
        var option = Option.Some("hello");
        
        // Act
        var result = option.Match(
            some: value => value.ToUpper(),
            none: () => "default"
        );
        
        // Assert
        await Assert.That(result).IsEqualTo("HELLO");
    }

    [Test]
    public async Task Match_none_calls_none_function()
    {
        // Arrange
        var option = Option.None<string>();
        
        // Act
        var result = option.Match(
            some: value => value.ToUpper(),
            none: () => "default"
        );
        
        // Assert
        await Assert.That(result).IsEqualTo("default");
    }

    [Test]
    public async Task Map_transforms_some_value()
    {
        // Arrange
        var option = Option.Some(5);
        
        // Act
        var result = option.Map(x => x * 2);
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var value = result.Match(some: x => x, none: () => 0);
        await Assert.That(value).IsEqualTo(10);
    }

    [Test]
    public async Task Map_preserves_none()
    {
        // Arrange
        var option = Option.None<int>();
        
        // Act
        var result = option.Map(x => x * 2);
        
        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Bind_chains_some_operations()
    {
        // Arrange
        var option = Option.Some(10);
        
        // Act
        var result = option.Bind(x => x > 5 ? Option.Some(x * 2) : Option.None<int>());
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var value = result.Match(some: x => x, none: () => 0);
        await Assert.That(value).IsEqualTo(20);
    }

    [Test]
    public async Task Bind_can_return_none()
    {
        // Arrange
        var option = Option.Some(3);
        
        // Act
        var result = option.Bind(x => x > 5 ? Option.Some(x * 2) : Option.None<int>());
        
        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Bind_preserves_none()
    {
        // Arrange
        var option = Option.None<int>();
        
        // Act
        var result = option.Bind(x => Option.Some(x * 2));
        
        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task Filter_keeps_value_when_predicate_true()
    {
        // Arrange
        var option = Option.Some(10);
        
        // Act
        var result = option.Filter(x => x > 5);
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
    }

    [Test]
    public async Task Filter_returns_none_when_predicate_false()
    {
        // Arrange
        var option = Option.Some(3);
        
        // Act
        var result = option.Filter(x => x > 5);
        
        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task GetValueOrDefault_returns_value_for_some()
    {
        // Arrange
        var option = Option.Some(42);
        
        // Act
        var result = option.GetValueOrDefault(0);
        
        // Assert
        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task GetValueOrDefault_returns_default_for_none()
    {
        // Arrange
        var option = Option.None<int>();
        
        // Act
        var result = option.GetValueOrDefault(99);
        
        // Assert
        await Assert.That(result).IsEqualTo(99);
    }

    [Test]
    public async Task Implicit_conversion_from_value_creates_some()
    {
        // Arrange & Act
        Option<string> option = "hello";
        
        // Assert
        await Assert.That(option.IsSome).IsTrue();
        var value = option.Match(some: x => x, none: () => "");
        await Assert.That(value).IsEqualTo("hello");
    }

    [Test]
    public async Task IEnumerable_yields_value_for_some()
    {
        // Arrange
        var option = Option.Some(42);
        
        // Act
        var list = option.ToList();
        
        // Assert
        await Assert.That(list.Count).IsEqualTo(1);
        await Assert.That(list[0]).IsEqualTo(42);
    }

    [Test]
    public async Task IEnumerable_yields_nothing_for_none()
    {
        // Arrange
        var option = Option.None<int>();
        
        // Act
        var list = option.ToList();
        
        // Assert
        await Assert.That(list.Count).IsEqualTo(0);
    }

    [Test]
    public async Task LINQ_select_transforms_some()
    {
        // Arrange
        var option = Option.Some(5);
        
        // Act
        var result = from x in option
                     select x * 2;
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var value = result.Match(some: x => x, none: () => 0);
        await Assert.That(value).IsEqualTo(10);
    }

    [Test]
    public async Task LINQ_selectmany_chains_operations()
    {
        // Arrange
        var option = Option.Some(5);
        
        // Act
        var result = from x in option
                     from y in Option.Some(x * 2)
                     select y + 1;
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var value = result.Match(some: x => x, none: () => 0);
        await Assert.That(value).IsEqualTo(11);
    }

    [Test]
    public async Task LINQ_where_filters_values()
    {
        // Arrange
        var option = Option.Some(10);
        
        // Act
        var result = from x in option
                     where x > 5
                     select x;
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
    }

    [Test]
    public async Task LINQ_where_returns_none_when_filter_fails()
    {
        // Arrange
        var option = Option.Some(3);
        
        // Act
        var result = from x in option
                     where x > 5
                     select x;
        
        // Assert
        await Assert.That(result.IsNone).IsTrue();
    }

    [Test]
    public async Task LINQ_on_enumerable_filters_none_values()
    {
        // Arrange
        var options = new[]
        {
            Option.Some(1),
            Option.None<int>(),
            Option.Some(2),
            Option.None<int>(),
            Option.Some(3)
        };
        
        // Act - SelectMany flattens and naturally filters out None
        var result = options.SelectMany(o => o).ToList();
        
        // Assert
        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo(1);
        await Assert.That(result[1]).IsEqualTo(2);
        await Assert.That(result[2]).IsEqualTo(3);
    }

    [Test]
    public async Task Choose_filters_none_and_unwraps_some()
    {
        // Arrange
        var options = new[]
        {
            Option.Some("a"),
            Option.None<string>(),
            Option.Some("b"),
            Option.None<string>(),
            Option.Some("c")
        };
        
        // Act
        var result = options.Choose().ToList();
        
        // Assert
        await Assert.That(result.Count).IsEqualTo(3);
        await Assert.That(result[0]).IsEqualTo("a");
        await Assert.That(result[1]).IsEqualTo("b");
        await Assert.That(result[2]).IsEqualTo("c");
    }

    [Test]
    public async Task ChooseMap_applies_function_and_filters()
    {
        // Arrange
        var numbers = new[] { 1, 2, 3, 4, 5 };
        
        // Act - Only keep even numbers
        var result = numbers.ChooseMap(n => n % 2 == 0 ? Option.Some(n) : Option.None<int>()).ToList();
        
        // Assert
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0]).IsEqualTo(2);
        await Assert.That(result[1]).IsEqualTo(4);
    }

    [Test]
    public async Task MapAsync_transforms_some_value_asynchronously()
    {
        // Arrange
        var option = Option.Some(5);
        
        // Act
        var result = await option.MapAsync(async x =>
        {
            await Task.Delay(1);
            return x * 2;
        });
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var value = result.Match(some: x => x, none: () => 0);
        await Assert.That(value).IsEqualTo(10);
    }

    [Test]
    public async Task BindAsync_chains_async_operations()
    {
        // Arrange
        var option = Option.Some(10);
        
        // Act
        var result = await option.BindAsync(async x =>
        {
            await Task.Delay(1);
            return x > 5 ? Option.Some(x * 2) : Option.None<int>();
        });
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var value = result.Match(some: x => x, none: () => 0);
        await Assert.That(value).IsEqualTo(20);
    }

    [Test]
    public async Task OrElse_returns_original_for_some()
    {
        // Arrange
        var option = Option.Some(42);
        var alternative = Option.Some(99);
        
        // Act
        var result = option.OrElse(alternative);
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var value = result.Match(some: x => x, none: () => 0);
        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task OrElse_returns_alternative_for_none()
    {
        // Arrange
        var option = Option.None<int>();
        var alternative = Option.Some(99);
        
        // Act
        var result = option.OrElse(alternative);
        
        // Assert
        await Assert.That(result.IsSome).IsTrue();
        var value = result.Match(some: x => x, none: () => 0);
        await Assert.That(value).IsEqualTo(99);
    }
}
