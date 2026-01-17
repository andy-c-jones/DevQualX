using DevQualX.Functional;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace DevQualX.Functional.Tests;

public class EitherShould
{
    #region Creation Tests
    
    [Test]
    public async Task Create_left_value()
    {
        var either = new Left<string, int>("left");
        
        await Assert.That(either.IsLeft).IsTrue();
        await Assert.That(either.IsRight).IsFalse();
    }
    
    [Test]
    public async Task Create_right_value()
    {
        var either = new Right<string, int>(42);
        
        await Assert.That(either.IsRight).IsTrue();
        await Assert.That(either.IsLeft).IsFalse();
    }
    
    [Test]
    public async Task Implicitly_convert_to_left()
    {
        Either<string, int> either = "left";
        
        await Assert.That(either.IsLeft).IsTrue();
    }
    
    [Test]
    public async Task Implicitly_convert_to_right()
    {
        Either<string, int> either = 42;
        
        await Assert.That(either.IsRight).IsTrue();
    }
    
    #endregion
    
    #region Match Tests
    
    [Test]
    public async Task Match_left_calls_onLeft()
    {
        var either = new Left<string, int>("error");
        
        var output = either.Match(
            left: s => $"Left: {s}",
            right: i => $"Right: {i}"
        );
        
        await Assert.That(output).IsEqualTo("Left: error");
    }
    
    [Test]
    public async Task Match_right_calls_onRight()
    {
        var either = new Right<string, int>(42);
        
        var output = either.Match(
            left: s => $"Left: {s}",
            right: i => $"Right: {i}"
        );
        
        await Assert.That(output).IsEqualTo("Right: 42");
    }
    
    [Test]
    public async Task MatchAsync_left_awaits_async_function()
    {
        var either = new Left<string, int>("error");
        
        var output = await either.MatchAsync(
            left: async s => { await Task.Delay(1); return $"Left: {s}"; },
            right: async i => { await Task.Delay(1); return $"Right: {i}"; }
        );
        
        await Assert.That(output).IsEqualTo("Left: error");
    }
    
    [Test]
    public async Task MatchAsync_right_awaits_async_function()
    {
        var either = new Right<string, int>(42);
        
        var output = await either.MatchAsync(
            left: async s => { await Task.Delay(1); return $"Left: {s}"; },
            right: async i => { await Task.Delay(1); return $"Right: {i}"; }
        );
        
        await Assert.That(output).IsEqualTo("Right: 42");
    }
    
    #endregion
    
    #region Map Tests
    
    [Test]
    public async Task MapLeft_transforms_left_value()
    {
        var either = new Left<string, int>("error");
        
        var mapped = either.MapLeft(s => s.ToUpper());
        
        await Assert.That(mapped.IsLeft).IsTrue();
        var value = mapped.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("ERROR");
    }
    
    [Test]
    public async Task MapLeft_preserves_right()
    {
        var either = new Right<string, int>(42);
        
        var mapped = either.MapLeft(s => s.ToUpper());
        
        await Assert.That(mapped.IsRight).IsTrue();
        var value = mapped.Match(_ => 0, i => i);
        await Assert.That(value).IsEqualTo(42);
    }
    
    [Test]
    public async Task MapRight_transforms_right_value()
    {
        var either = new Right<string, int>(42);
        
        var mapped = either.MapRight(i => i * 2);
        
        await Assert.That(mapped.IsRight).IsTrue();
        var value = mapped.Match(_ => 0, i => i);
        await Assert.That(value).IsEqualTo(84);
    }
    
    [Test]
    public async Task MapRight_preserves_left()
    {
        var either = new Left<string, int>("error");
        
        var mapped = either.MapRight(i => i * 2);
        
        await Assert.That(mapped.IsLeft).IsTrue();
        var value = mapped.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("error");
    }
    
    [Test]
    public async Task Map_transforms_both_sides()
    {
        var left = new Left<string, int>("error");
        var right = new Right<string, int>(42);
        
        var mappedLeft = left.Map(s => s.ToUpper(), i => i * 2);
        var mappedRight = right.Map(s => s.ToUpper(), i => i * 2);
        
        await Assert.That(mappedLeft.IsLeft).IsTrue();
        await Assert.That(mappedLeft.Match(s => s, _ => "")).IsEqualTo("ERROR");
        
        await Assert.That(mappedRight.IsRight).IsTrue();
        await Assert.That(mappedRight.Match(_ => 0, i => i)).IsEqualTo(84);
    }
    
    [Test]
    public async Task MapLeftAsync_transforms_with_async_function()
    {
        var either = new Left<string, int>("error");
        
        var mapped = await either.MapLeftAsync(async s => { await Task.Delay(1); return s.ToUpper(); });
        
        await Assert.That(mapped.IsLeft).IsTrue();
        var value = mapped.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("ERROR");
    }
    
    [Test]
    public async Task MapRightAsync_transforms_with_async_function()
    {
        var either = new Right<string, int>(42);
        
        var mapped = await either.MapRightAsync(async i => { await Task.Delay(1); return i * 2; });
        
        await Assert.That(mapped.IsRight).IsTrue();
        var value = mapped.Match(_ => 0, i => i);
        await Assert.That(value).IsEqualTo(84);
    }
    
    #endregion
    
    #region Bind Tests
    
    [Test]
    public async Task Bind_chains_right_values()
    {
        var either = new Right<string, int>(42);
        
        var bound = either.Bind(i => new Right<string, string>($"Value: {i}"));
        
        await Assert.That(bound.IsRight).IsTrue();
        var value = bound.Match(_ => "", s => s);
        await Assert.That(value).IsEqualTo("Value: 42");
    }
    
    [Test]
    public async Task Bind_propagates_left()
    {
        var either = new Left<string, int>("error");
        
        var bound = either.Bind(i => new Right<string, string>($"Value: {i}"));
        
        await Assert.That(bound.IsLeft).IsTrue();
        var value = bound.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("error");
    }
    
    [Test]
    public async Task Bind_can_switch_to_left()
    {
        var either = new Right<string, int>(42);
        
        var bound = either.Bind(i => new Left<string, string>("new error"));
        
        await Assert.That(bound.IsLeft).IsTrue();
        var value = bound.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("new error");
    }
    
    [Test]
    public async Task BindAsync_chains_async_operations()
    {
        var either = new Right<string, int>(42);
        
        var bound = await either.BindAsync<string>(async i => 
        { 
            await Task.Delay(1); 
            return new Right<string, string>($"Value: {i}"); 
        });
        
        await Assert.That(bound.IsRight).IsTrue();
        var value = bound.Match(_ => "", s => s);
        await Assert.That(value).IsEqualTo("Value: 42");
    }
    
    #endregion
    
    #region IfLeft/IfRight Tests
    
    [Test]
    public async Task IfLeft_executes_action_on_left()
    {
        var either = new Left<string, int>("error");
        var executed = false;
        
        either.IfLeft(s => executed = true);
        
        await Assert.That(executed).IsTrue();
    }
    
    [Test]
    public async Task IfLeft_does_not_execute_on_right()
    {
        var either = new Right<string, int>(42);
        var executed = false;
        
        either.IfLeft(s => executed = true);
        
        await Assert.That(executed).IsFalse();
    }
    
    [Test]
    public async Task IfRight_executes_action_on_right()
    {
        var either = new Right<string, int>(42);
        var executed = false;
        
        either.IfRight(i => executed = true);
        
        await Assert.That(executed).IsTrue();
    }
    
    [Test]
    public async Task IfRight_does_not_execute_on_left()
    {
        var either = new Left<string, int>("error");
        var executed = false;
        
        either.IfRight(i => executed = true);
        
        await Assert.That(executed).IsFalse();
    }
    
    #endregion
    
    #region Swap Tests
    
    [Test]
    public async Task Swap_converts_left_to_right()
    {
        var either = new Left<string, int>("error");
        
        var swapped = either.Swap();
        
        await Assert.That(swapped.IsRight).IsTrue();
        await Assert.That(swapped.IsLeft).IsFalse();
        var value = swapped.Match(_ => "", s => s);
        await Assert.That(value).IsEqualTo("error");
    }
    
    [Test]
    public async Task Swap_converts_right_to_left()
    {
        var either = new Right<string, int>(42);
        
        var swapped = either.Swap();
        
        await Assert.That(swapped.IsLeft).IsTrue();
        await Assert.That(swapped.IsRight).IsFalse();
        var value = swapped.Match(i => i, _ => 0);
        await Assert.That(value).IsEqualTo(42);
    }
    
    #endregion
    
    #region LINQ Tests
    
    [Test]
    public async Task Select_transforms_right_value()
    {
        var either = new Right<string, int>(42);
        
        var query = from x in either
                    select x * 2;
        
        await Assert.That(query.IsRight).IsTrue();
        var value = query.Match(_ => 0, i => i);
        await Assert.That(value).IsEqualTo(84);
    }
    
    [Test]
    public async Task Select_preserves_left()
    {
        var either = new Left<string, int>("error");
        
        var query = from x in either
                    select x * 2;
        
        await Assert.That(query.IsLeft).IsTrue();
        var value = query.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("error");
    }
    
    [Test]
    public async Task SelectMany_chains_right_values()
    {
        var either1 = new Right<string, int>(10);
        var either2 = new Right<string, int>(20);
        
        var query = from x in either1
                    from y in either2
                    select x + y;
        
        await Assert.That(query.IsRight).IsTrue();
        var value = query.Match(_ => 0, i => i);
        await Assert.That(value).IsEqualTo(30);
    }
    
    [Test]
    public async Task SelectMany_short_circuits_on_first_left()
    {
        var either1 = new Left<string, int>("first error");
        var either2 = new Right<string, int>(20);
        
        var query = from x in either1
                    from y in either2
                    select x + y;
        
        await Assert.That(query.IsLeft).IsTrue();
        var value = query.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("first error");
    }
    
    [Test]
    public async Task SelectMany_short_circuits_on_second_left()
    {
        var either1 = new Right<string, int>(10);
        var either2 = new Left<string, int>("second error");
        
        var query = from x in either1
                    from y in either2
                    select x + y;
        
        await Assert.That(query.IsLeft).IsTrue();
        var value = query.Match(s => s, _ => "");
        await Assert.That(value).IsEqualTo("second error");
    }
    
    #endregion
}
