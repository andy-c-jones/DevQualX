namespace DevQualX.Domain.Models;

/// <summary>
/// Result of processing a report.
/// </summary>
public record ProcessingResult(
    bool Success,
    bool ShouldRetry,
    string? FailureReason = null);
