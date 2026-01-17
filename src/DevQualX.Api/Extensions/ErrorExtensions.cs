using DevQualX.Functional;
using Microsoft.AspNetCore.Mvc;

namespace DevQualX.Api.Extensions;

/// <summary>
/// Extension methods for converting Error types to ASP.NET Core responses.
/// </summary>
public static class ErrorExtensions
{
    /// <summary>
    /// Converts an Error to ProblemDetails with appropriate HTTP status code.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>ProblemDetails with status code, title, and detail mapped from the error.</returns>
    public static ProblemDetails ToProblemDetails(this Error error)
    {
        var (status, title) = error switch
        {
            ValidationError => (StatusCodes.Status400BadRequest, "Validation Error"),
            BadRequestError => (StatusCodes.Status400BadRequest, "Bad Request"),
            UnauthorizedError => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ForbiddenError => (StatusCodes.Status403Forbidden, "Forbidden"),
            NotFoundError => (StatusCodes.Status404NotFound, "Not Found"),
            ConflictError => (StatusCodes.Status409Conflict, "Conflict"),
            ExternalServiceError => (StatusCodes.Status502BadGateway, "External Service Error"),
            InternalError => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
            _ => (StatusCodes.Status500InternalServerError, "Error")
        };

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = error.Message
        };

        // Add error code if present
        if (!string.IsNullOrEmpty(error.Code))
        {
            problemDetails.Extensions["code"] = error.Code;
        }

        // Add metadata if present
        if (error.Metadata is not null && error.Metadata.Count > 0)
        {
            problemDetails.Extensions["metadata"] = error.Metadata;
        }

        // Add validation errors if present
        if (error is ValidationError validationError && validationError.Errors is not null)
        {
            problemDetails.Extensions["errors"] = validationError.Errors;
        }

        // Add resource information for NotFoundError
        if (error is NotFoundError notFoundError)
        {
            if (!string.IsNullOrEmpty(notFoundError.ResourceType))
            {
                problemDetails.Extensions["resourceType"] = notFoundError.ResourceType;
            }
            if (!string.IsNullOrEmpty(notFoundError.ResourceId))
            {
                problemDetails.Extensions["resourceId"] = notFoundError.ResourceId;
            }
        }

        // Add service information for ExternalServiceError
        if (error is ExternalServiceError externalServiceError)
        {
            if (!string.IsNullOrEmpty(externalServiceError.ServiceName))
            {
                problemDetails.Extensions["serviceName"] = externalServiceError.ServiceName;
            }
            if (!string.IsNullOrEmpty(externalServiceError.InnerMessage))
            {
                problemDetails.Extensions["innerMessage"] = externalServiceError.InnerMessage;
            }
        }

        return problemDetails;
    }
}
