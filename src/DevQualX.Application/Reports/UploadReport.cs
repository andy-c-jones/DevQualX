using DevQualX.Domain.Infrastructure;
using DevQualX.Domain.Models;
using Microsoft.Extensions.Logging;

namespace DevQualX.Application.Reports;

public class UploadReport(
    IBlobStorageService blobStorageService,
    IMessageQueueService messageQueueService,
    ILogger<UploadReport> logger) : IUploadReport
{
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100MB
    private static readonly string[] AllowedContentTypes = 
    [
        "application/pdf",
        "application/zip",
        "application/x-zip-compressed",
        "application/json",
        "text/plain",
        "text/csv"
    ];

    public async Task<ReportMetadata> ExecuteAsync(
        ReportUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate file size
        if (request.FileSizeBytes > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size {request.FileSizeBytes} bytes exceeds maximum allowed size of {MaxFileSizeBytes} bytes");
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(request.ContentType.ToLowerInvariant()))
        {
            throw new InvalidOperationException($"Content type {request.ContentType} is not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}");
        }

        logger.LogInformation("Uploading report: {Organisation}/{Project}/{FileName}", 
            request.Organisation, request.Project, request.FileName);

        // Upload to blob storage (decompresses Brotli internally)
        var metadata = await blobStorageService.UploadReportAsync(
            request.Organisation,
            request.Project,
            request.FileName,
            request.ContentType,
            request.Content,
            cancellationToken: cancellationToken);

        // Send message to queue
        await messageQueueService.SendMessageAsync(
            "reports",
            metadata,
            new Dictionary<string, object> { ["AttemptCount"] = 1 },
            cancellationToken);

        logger.LogInformation("Report uploaded and queued: {BlobUrl}", metadata.BlobUrl);

        return metadata;
    }
}
