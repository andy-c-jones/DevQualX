using DevQualX.Domain.Models;

namespace DevQualX.Domain.Infrastructure;

/// <summary>
/// Service for blob storage operations.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a report to blob storage.
    /// </summary>
    /// <param name="organisation">Organisation name</param>
    /// <param name="project">Project name</param>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">Content type</param>
    /// <param name="content">File content stream (will be decompressed if Brotli)</param>
    /// <param name="metadata">Optional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Report metadata including blob URL and path</returns>
    Task<ReportMetadata> UploadReportAsync(
        string organisation,
        string project,
        string fileName,
        string contentType,
        Stream content,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the virus scan status of a blob.
    /// </summary>
    Task<VirusScanStatus> GetVirusScanStatusAsync(
        string blobUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the reports container exists with proper configuration.
    /// </summary>
    Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default);
}
