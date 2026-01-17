using DevQualX.Functional;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DevQualX.Api.Extensions;

/// <summary>
/// Extension methods for converting Result types to ASP.NET Core IResult.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an IResult for use in minimal API endpoints.
    /// Success returns 200 OK with the value, failure returns a ProblemDetails response.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IResult representing the success or failure.</returns>
    public static IResult ToHttpResult<T, TError>(this Result<T, TError> result) 
        where TError : Error
    {
        return result.Match<IResult>(
            success: value => TypedResults.Ok(value),
            failure: error => TypedResults.Problem(error.ToProblemDetails())
        );
    }

    /// <summary>
    /// Converts a Result to an IResult with a custom success status code.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="successStatusCode">The HTTP status code to return on success.</param>
    /// <returns>An IResult representing the success or failure.</returns>
    public static IResult ToHttpResult<T, TError>(this Result<T, TError> result, int successStatusCode) 
        where TError : Error
    {
        return result.Match<IResult>(
            success: value => TypedResults.Json(value, statusCode: successStatusCode),
            failure: error => TypedResults.Problem(error.ToProblemDetails())
        );
    }

    /// <summary>
    /// Converts a Result to a Created response (201) at the specified location.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="uri">The URI of the created resource.</param>
    /// <returns>An IResult representing the success or failure.</returns>
    public static IResult ToCreatedResult<T, TError>(this Result<T, TError> result, string uri) 
        where TError : Error
    {
        return result.Match<IResult>(
            success: value => TypedResults.Created(uri, value),
            failure: error => TypedResults.Problem(error.ToProblemDetails())
        );
    }

    /// <summary>
    /// Converts a Result to a Created response (201) at a URI generated from the value.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="uriFactory">Function to generate the URI from the success value.</param>
    /// <returns>An IResult representing the success or failure.</returns>
    public static IResult ToCreatedResult<T, TError>(this Result<T, TError> result, Func<T, string> uriFactory) 
        where TError : Error
    {
        return result.Match<IResult>(
            success: value => TypedResults.Created(uriFactory(value), value),
            failure: error => TypedResults.Problem(error.ToProblemDetails())
        );
    }

    /// <summary>
    /// Converts a Result with no meaningful value to a NoContent response (204) on success.
    /// </summary>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IResult representing the success or failure.</returns>
    public static IResult ToNoContentResult<TError>(this Result<bool, TError> result) 
        where TError : Error
    {
        return result.Match<IResult>(
            success: _ => TypedResults.NoContent(),
            failure: error => TypedResults.Problem(error.ToProblemDetails())
        );
    }

    /// <summary>
    /// Converts a Result representing a deletion to an appropriate response.
    /// Returns 204 No Content on success, or a ProblemDetails response on failure.
    /// </summary>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An IResult representing the success or failure.</returns>
    public static IResult ToDeleteResult<TError>(this Result<bool, TError> result) 
        where TError : Error
    {
        return result.ToNoContentResult();
    }
}
