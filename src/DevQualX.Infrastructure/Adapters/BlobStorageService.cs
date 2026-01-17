using System.IO.Compression;
using System.Security.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DevQualX.Domain.Infrastructure;
using DevQualX.Domain.Models;
using Microsoft.Extensions.Logging;

namespace DevQualX.Infrastructure.Adapters;

public class BlobStorageService(
    BlobServiceClient blobServiceClient,
    ILogger<BlobStorageService> logger) : IBlobStorageService
{
    private const string ContainerName = "reports";

    public async Task<ReportMetadata> UploadReportAsync(
        string organisation,
        string project,
        string fileName,
        string contentType,
        Stream content,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureContainerExistsAsync(cancellationToken);

        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var blobPath = $"{organisation}/{project}/{timestamp}_{fileName}";
        var blobClient = containerClient.GetBlobClient(blobPath);

        // Decompress Brotli and calculate checksum
        await using var decompressedStream = new MemoryStream();
        await using (var brotliStream = new BrotliStream(content, CompressionMode.Decompress, leaveOpen: false))
        {
            await brotliStream.CopyToAsync(decompressedStream, cancellationToken);
        }
        
        decompressedStream.Position = 0;
        var checksum = await CalculateChecksumAsync(decompressedStream, cancellationToken);
        decompressedStream.Position = 0;

        // Upload to blob storage
        var blobMetadata = new Dictionary<string, string>
        {
            ["Organisation"] = organisation,
            ["Project"] = project,
            ["OriginalFileName"] = fileName,
            ["ContentType"] = contentType,
            ["Checksum"] = checksum,
            ["UploadedAt"] = DateTimeOffset.UtcNow.ToString("O")
        };

        if (metadata != null)
        {
            foreach (var (key, value) in metadata)
            {
                blobMetadata[$"Custom_{key}"] = value;
            }
        }

        var uploadOptions = new BlobUploadOptions
        {
            Metadata = blobMetadata,
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        await blobClient.UploadAsync(decompressedStream, uploadOptions, cancellationToken);

        logger.LogInformation("Uploaded report to blob storage: {BlobPath}", blobPath);

        return new ReportMetadata(
            organisation,
            project,
            fileName,
            blobClient.Uri.ToString(),
            blobPath,
            contentType,
            decompressedStream.Length,
            checksum,
            DateTimeOffset.UtcNow,
            metadata);
    }

    public async Task<VirusScanStatus> GetVirusScanStatusAsync(
        string blobUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            // Check for Microsoft Defender for Storage tags
            if (properties.Value.Metadata.TryGetValue("Malware Scanning scan result", out var scanResult))
            {
                return scanResult switch
                {
                    "No threats found" => VirusScanStatus.Clean,
                    "Malicious" => VirusScanStatus.Infected,
                    _ => VirusScanStatus.Error
                };
            }

            // Check for custom scan status metadata
            if (properties.Value.Metadata.TryGetValue("ScanStatus", out var status))
            {
                return Enum.TryParse<VirusScanStatus>(status, out var parsed)
                    ? parsed
                    : VirusScanStatus.NotScanned;
            }

            // Azurite doesn't support virus scanning
            return VirusScanStatus.NotScanned;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking virus scan status for {BlobUrl}", blobUrl);
            return VirusScanStatus.Error;
        }
    }

    public async Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        
        logger.LogInformation("Ensured reports container exists");
    }

    private static async Task<string> CalculateChecksumAsync(Stream stream, CancellationToken cancellationToken)
    {
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }
}
