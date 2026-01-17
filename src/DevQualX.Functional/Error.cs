namespace DevQualX.Functional;

/// <summary>
/// Base class for all application errors.
/// Use this as a discriminated union by inheriting for specific error types.
/// </summary>
public abstract record Error
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets or sets an optional error code for categorization.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Gets or sets optional metadata for additional error information.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Represents a validation error (HTTP 400).
/// </summary>
public sealed record ValidationError : Error
{
    /// <summary>
    /// Gets or sets field-specific validation errors.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }
}

/// <summary>
/// Represents a resource not found error (HTTP 404).
/// </summary>
public sealed record NotFoundError : Error
{
    /// <summary>
    /// Gets or sets the type of resource that was not found.
    /// </summary>
    public string? ResourceType { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the resource that was not found.
    /// </summary>
    public string? ResourceId { get; init; }
}

/// <summary>
/// Represents an unauthorized access error (HTTP 401).
/// </summary>
public sealed record UnauthorizedError : Error
{
    /// <summary>
    /// Gets or sets the reason for authorization failure.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Represents a forbidden access error (HTTP 403).
/// </summary>
public sealed record ForbiddenError : Error
{
    /// <summary>
    /// Gets or sets the resource or action that was forbidden.
    /// </summary>
    public string? Resource { get; init; }
}

/// <summary>
/// Represents a conflict error, such as duplicate resources (HTTP 409).
/// </summary>
public sealed record ConflictError : Error
{
    /// <summary>
    /// Gets or sets the conflicting resource identifier.
    /// </summary>
    public string? ConflictingResource { get; init; }
}

/// <summary>
/// Represents an error from an external service or dependency.
/// </summary>
public sealed record ExternalServiceError : Error
{
    /// <summary>
    /// Gets or sets the name of the external service.
    /// </summary>
    public string? ServiceName { get; init; }

    /// <summary>
    /// Gets or sets the inner exception message from the external service.
    /// </summary>
    public string? InnerMessage { get; init; }
}

/// <summary>
/// Represents a bad request error (HTTP 400).
/// </summary>
public sealed record BadRequestError : Error
{
    /// <summary>
    /// Gets or sets the specific parameter or field that caused the error.
    /// </summary>
    public string? Parameter { get; init; }
}

/// <summary>
/// Represents a generic internal server error (HTTP 500).
/// Use sparingly - prefer specific error types.
/// </summary>
public sealed record InternalError : Error
{
    /// <summary>
    /// Gets or sets the exception type name if applicable.
    /// </summary>
    public string? ExceptionType { get; init; }
}
