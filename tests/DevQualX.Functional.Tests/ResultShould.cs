using DevQualX.Functional;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace DevQualX.Functional.Tests;

public class ResultShould
{
    #region Creation Tests
    
    [Test]
    public async Task Create_success_result_with_value()
    {
        var result = new Success<int, Error>(42);
        
        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.IsFailure).IsFalse();
    }
    
    [Test]
    public async Task Create_failure_result_with_error()
    {
        var error = new ValidationError { Message = "Invalid input" };
        var result = new Failure<int, Error>(error);
        
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.IsSuccess).IsFalse();
    }
    
    [Test]
    public async Task Implicitly_convert_value_to_success()
    {
        Result<int, Error> result = 42;
        
        await Assert.That(result.IsSuccess).IsTrue();
    }
    
    [Test]
    public async Task Implicitly_convert_error_to_failure()
    {
        Result<int, Error> result = new ValidationError { Message = "Error" };
        
        await Assert.That(result.IsFailure).IsTrue();
    }
    
    #endregion
    
    #region Match Tests
    
    [Test]
    public async Task Match_success_calls_onSuccess()
    {
        var result = new Success<int, Error>(42);
        
        var output = result.Match(
            success: value => $"Success: {value}",
            failure: error => $"Failure: {error.Message}"
        );
        
        await Assert.That(output).IsEqualTo("Success: 42");
    }
    
    [Test]
    public async Task Match_failure_calls_onFailure()
    {
        var result = new Failure<int, Error>(new ValidationError { Message = "Invalid" });
        
        var output = result.Match(
            success: value => $"Success: {value}",
            failure: error => $"Failure: {error.Message}"
        );
        
        await Assert.That(output).IsEqualTo("Failure: Invalid");
    }
    
    [Test]
    public async Task MatchAsync_success_awaits_async_function()
    {
        var result = new Success<int, Error>(42);
        
        var output = await result.MatchAsync(
            success: async value => { await Task.Delay(1); return $"Success: {value}"; },
            failure: async error => { await Task.Delay(1); return $"Failure: {error.Message}"; }
        );
        
        await Assert.That(output).IsEqualTo("Success: 42");
    }
    
    #endregion
    
    #region Map Tests
    
    [Test]
    public async Task Map_transforms_success_value()
    {
        var result = new Success<int, Error>(42);
        
        var mapped = result.Map(x => x * 2);
        
        await Assert.That(mapped.IsSuccess).IsTrue();
        var value = mapped.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(84);
    }
    
    [Test]
    public async Task Map_preserves_failure()
    {
        var error = new ValidationError { Message = "Invalid" };
        var result = new Failure<int, Error>(error);
        
        var mapped = result.Map(x => x * 2);
        
        await Assert.That(mapped.IsFailure).IsTrue();
        var errorResult = mapped.Match(_ => new ValidationError { Message = "" }, e => e);
        await Assert.That(errorResult.Message).IsEqualTo("Invalid");
    }
    
    [Test]
    public async Task MapAsync_transforms_success_with_async_function()
    {
        var result = new Success<int, Error>(42);
        
        var mapped = await result.MapAsync(async x => { await Task.Delay(1); return x * 2; });
        
        await Assert.That(mapped.IsSuccess).IsTrue();
        var value = mapped.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(84);
    }
    
    [Test]
    public async Task MapError_transforms_failure_error()
    {
        var result = new Failure<int, ValidationError>(new ValidationError { Message = "Invalid" });
        
        var mapped = result.MapError(e => new NotFoundError { Message = $"Not found: {e.Message}" });
        
        await Assert.That(mapped.IsFailure).IsTrue();
        var error = mapped.Match(_ => new NotFoundError { Message = "" }, e => e);
        await Assert.That(error).IsTypeOf<NotFoundError>();
        await Assert.That(error.Message).IsEqualTo("Not found: Invalid");
    }
    
    [Test]
    public async Task MapError_preserves_success()
    {
        var result = new Success<int, ValidationError>(42);
        
        var mapped = result.MapError(e => new NotFoundError { Message = $"Not found: {e.Message}" });
        
        await Assert.That(mapped.IsSuccess).IsTrue();
        var value = mapped.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(42);
    }
    
    #endregion
    
    #region Bind Tests
    
    [Test]
    public async Task Bind_chains_success_results()
    {
        var result = new Success<int, Error>(42);
        
        var bound = result.Bind(x => new Success<string, Error>($"Value: {x}"));
        
        await Assert.That(bound.IsSuccess).IsTrue();
        var value = bound.Match(v => v, _ => "");
        await Assert.That(value).IsEqualTo("Value: 42");
    }
    
    [Test]
    public async Task Bind_propagates_initial_failure()
    {
        var error = new ValidationError { Message = "Invalid" };
        var result = new Failure<int, Error>(error);
        
        var bound = result.Bind(x => new Success<string, Error>($"Value: {x}"));
        
        await Assert.That(bound.IsFailure).IsTrue();
        var errorResult = bound.Match(_ => new ValidationError { Message = "" }, e => e);
        await Assert.That(errorResult.Message).IsEqualTo("Invalid");
    }
    
    [Test]
    public async Task Bind_propagates_secondary_failure()
    {
        var result = new Success<int, Error>(42);
        
        var bound = result.Bind(_ => new Failure<string, Error>(new NotFoundError { Message = "Not found" }));
        
        await Assert.That(bound.IsFailure).IsTrue();
        var error = bound.Match(_ => new NotFoundError { Message = "" }, e => e);
        await Assert.That(error.Message).IsEqualTo("Not found");
    }
    
    [Test]
    public async Task BindAsync_chains_async_operations()
    {
        var result = new Success<int, Error>(42);
        
        var bound = await result.BindAsync<string>(async x => 
        { 
            await Task.Delay(1); 
            return new Success<string, Error>($"Value: {x}"); 
        });
        
        await Assert.That(bound.IsSuccess).IsTrue();
        var value = bound.Match(v => v, _ => "");
        await Assert.That(value).IsEqualTo("Value: 42");
    }
    
    #endregion
    
    #region Tap Tests
    
    [Test]
    public async Task Tap_executes_action_on_success()
    {
        var result = new Success<int, Error>(42);
        var executed = false;
        
        var tapped = result.Tap(x => executed = true);
        
        await Assert.That(executed).IsTrue();
        await Assert.That(tapped.IsSuccess).IsTrue();
    }
    
    [Test]
    public async Task Tap_does_not_execute_on_failure()
    {
        var result = new Failure<int, Error>(new ValidationError { Message = "Invalid" });
        var executed = false;
        
        var tapped = result.Tap(x => executed = true);
        
        await Assert.That(executed).IsFalse();
        await Assert.That(tapped.IsFailure).IsTrue();
    }
    
    [Test]
    public async Task TapError_executes_action_on_failure()
    {
        var result = new Failure<int, Error>(new ValidationError { Message = "Invalid" });
        var executed = false;
        
        var tapped = result.TapError(e => executed = true);
        
        await Assert.That(executed).IsTrue();
        await Assert.That(tapped.IsFailure).IsTrue();
    }
    
    [Test]
    public async Task TapError_does_not_execute_on_success()
    {
        var result = new Success<int, Error>(42);
        var executed = false;
        
        var tapped = result.TapError(e => executed = true);
        
        await Assert.That(executed).IsFalse();
        await Assert.That(tapped.IsSuccess).IsTrue();
    }
    
    #endregion
    
    #region GetValueOrDefault Tests
    
    [Test]
    public async Task GetValueOrDefault_returns_value_on_success()
    {
        var result = new Success<int, Error>(42);
        
        var value = result.GetValueOrDefault(0);
        
        await Assert.That(value).IsEqualTo(42);
    }
    
    [Test]
    public async Task GetValueOrDefault_returns_default_on_failure()
    {
        var result = new Failure<int, Error>(new ValidationError { Message = "Invalid" });
        
        var value = result.GetValueOrDefault(0);
        
        await Assert.That(value).IsEqualTo(0);
    }
    
    [Test]
    public async Task GetValueOrDefault_with_parameter_returns_fallback_on_failure()
    {
        var result = new Failure<int, Error>(new ValidationError { Message = "Invalid" });
        
        var value = result.GetValueOrDefault(99);
        
        await Assert.That(value).IsEqualTo(99);
    }
    
    #endregion
    
    #region OrElse Tests
    
    [Test]
    public async Task OrElse_returns_original_on_success()
    {
        var result = new Success<int, Error>(42);
        
        var alternative = result.OrElse(new Success<int, Error>(99));
        
        await Assert.That(alternative.IsSuccess).IsTrue();
        var value = alternative.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(42);
    }
    
    [Test]
    public async Task OrElse_returns_alternative_on_failure()
    {
        var result = new Failure<int, Error>(new ValidationError { Message = "Invalid" });
        
        var alternative = result.OrElse(new Success<int, Error>(99));
        
        await Assert.That(alternative.IsSuccess).IsTrue();
        var value = alternative.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(99);
    }
    
    #endregion
    
    #region AsOption Tests
    
    [Test]
    public async Task AsOption_converts_success_to_some()
    {
        var result = new Success<int, Error>(42);
        
        var option = result.AsOption();
        
        await Assert.That(option.IsSome).IsTrue();
        var value = option.Match(v => v, () => 0);
        await Assert.That(value).IsEqualTo(42);
    }
    
    [Test]
    public async Task AsOption_converts_failure_to_none()
    {
        var result = new Failure<int, Error>(new ValidationError { Message = "Invalid" });
        
        var option = result.AsOption();
        
        await Assert.That(option.IsNone).IsTrue();
    }
    
    #endregion
    
    #region LINQ Tests
    
    [Test]
    public async Task Select_transforms_success_value()
    {
        var result = new Success<int, Error>(42);
        
        var query = from x in result
                    select x * 2;
        
        await Assert.That(query.IsSuccess).IsTrue();
        var value = query.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(84);
    }
    
    [Test]
    public async Task SelectMany_chains_results()
    {
        var result1 = new Success<int, Error>(10);
        var result2 = new Success<int, Error>(20);
        
        var query = from x in result1
                    from y in result2
                    select x + y;
        
        await Assert.That(query.IsSuccess).IsTrue();
        var value = query.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(30);
    }
    
    [Test]
    public async Task SelectMany_short_circuits_on_first_failure()
    {
        var result1 = new Failure<int, Error>(new ValidationError { Message = "First error" });
        var result2 = new Success<int, Error>(20);
        
        var query = from x in result1
                    from y in result2
                    select x + y;
        
        await Assert.That(query.IsFailure).IsTrue();
        var error = query.Match(_ => new ValidationError { Message = "" }, e => e);
        await Assert.That(error.Message).IsEqualTo("First error");
    }
    
    [Test]
    public async Task SelectMany_short_circuits_on_second_failure()
    {
        var result1 = new Success<int, Error>(10);
        var result2 = new Failure<int, Error>(new ValidationError { Message = "Second error" });
        
        var query = from x in result1
                    from y in result2
                    select x + y;
        
        await Assert.That(query.IsFailure).IsTrue();
        var error = query.Match(_ => new ValidationError { Message = "" }, e => e);
        await Assert.That(error.Message).IsEqualTo("Second error");
    }
    
    [Test]
    public async Task Complex_query_with_multiple_results()
    {
        var result1 = new Success<int, Error>(5);
        var result2 = new Success<int, Error>(10);
        var result3 = new Success<int, Error>(15);
        
        var query = from x in result1
                    from y in result2
                    from z in result3
                    select x + y + z;
        
        await Assert.That(query.IsSuccess).IsTrue();
        var value = query.Match(v => v, _ => 0);
        await Assert.That(value).IsEqualTo(30);
    }
    
    #endregion
    
    #region Error Type Tests
    
    [Test]
    public async Task ValidationError_contains_correct_properties()
    {
        var error = new ValidationError 
        { 
            Message = "Field is required", 
            Code = "VAL001", 
            Metadata = new Dictionary<string, object> { ["Field"] = "Email" }
        };
        
        await Assert.That(error.Message).IsEqualTo("Field is required");
        await Assert.That(error.Code).IsEqualTo("VAL001");
        await Assert.That(error.Metadata).IsNotNull();
        await Assert.That(error.Metadata!["Field"]).IsEqualTo("Email");
    }
    
    [Test]
    public async Task NotFoundError_is_error_subtype()
    {
        var error = new NotFoundError { Message = "User not found" };
        
        await Assert.That(error).IsTypeOf<NotFoundError>();
        await Assert.That(error).IsAssignableTo<Error>();
    }
    
    [Test]
    public async Task All_error_types_are_error_subtypes()
    {
        Error[] errors = 
        [
            new ValidationError { Message = "Validation" },
            new NotFoundError { Message = "Not found" },
            new UnauthorizedError { Message = "Unauthorized" },
            new ForbiddenError { Message = "Forbidden" },
            new ConflictError { Message = "Conflict" },
            new ExternalServiceError { Message = "External" },
            new BadRequestError { Message = "Bad request" },
            new InternalError { Message = "Internal" }
        ];
        
        foreach (var error in errors)
        {
            await Assert.That(error).IsAssignableTo<Error>();
        }
    }
    
    #endregion
}
