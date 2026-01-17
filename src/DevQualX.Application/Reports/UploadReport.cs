using DevQualX.Domain.Infrastructure;
using DevQualX.Domain.Models;
using DevQualX.Functional;
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

    public async Task<Result<ReportMetadata, Error>> ExecuteAsync(
        ReportUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate file size
        if (request.FileSizeBytes > MaxFileSizeBytes)
        {
            return new ValidationError
            {
                Message = $"File size {request.FileSizeBytes} bytes exceeds maximum allowed size of {MaxFileSizeBytes} bytes",
                Code = "FILE_TOO_LARGE"
            };
        }

        // Validate content type
        if (!AllowedContentTypes.Contains(request.ContentType.ToLowerInvariant()))
        {
            return new ValidationError
            {
                Message = $"Content type {request.ContentType} is not allowed",
                Code = "INVALID_CONTENT_TYPE",
                Errors = new Dictionary<string, string[]>
                {
                    ["ContentType"] = [$"Allowed types: {string.Join(", ", AllowedContentTypes)}"]
                }
            };
        }

        logger.LogInformation(
            "User {UserId} uploading report to installation {InstallationId}: {Organisation}/{Project}/{FileName}",
            request.UserId, request.InstallationId, request.Organisation, request.Project, request.FileName);

        try
        {
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
                new Dictionary<string, object> 
                { 
                    ["AttemptCount"] = 1,
                    ["UserId"] = request.UserId,
                    ["InstallationId"] = request.InstallationId
                },
                cancellationToken);

            logger.LogInformation(
                "Report uploaded and queued by user {UserId}: {BlobUrl}",
                request.UserId, metadata.BlobUrl);

            return metadata;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload report for user {UserId}", request.UserId);
            return new InternalError
            {
                Message = "Failed to upload report",
                Code = "UPLOAD_FAILED"
            };
        }
    }
}
