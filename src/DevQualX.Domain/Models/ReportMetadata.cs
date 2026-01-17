namespace DevQualX.Domain.Models;

/// <summary>
/// Metadata about an uploaded report.
/// </summary>
public record ReportMetadata(
    string Organisation,
    string Project,
    string FileName,
    string BlobUrl,
    string BlobPath,
    string ContentType,
    long FileSizeBytes,
    string Checksum,
    DateTimeOffset UploadedAt,
    Dictionary<string, string>? Metadata = null);
