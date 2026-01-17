namespace DevQualX.Domain.Models;

/// <summary>
/// Request to upload a report file.
/// </summary>
public record ReportUploadRequest(
    long UserId,
    int InstallationId,
    string Organisation,
    string Project,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Stream Content);
