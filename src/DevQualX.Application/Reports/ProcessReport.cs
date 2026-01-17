using DevQualX.Domain.Infrastructure;
using DevQualX.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DevQualX.Application.Reports;

public class ProcessReport(
    IBlobStorageService blobStorageService,
    IConfiguration configuration,
    ILogger<ProcessReport> logger) : IProcessReport
{
    public async Task<ProcessingResult> ExecuteAsync(
        ReportMetadata reportMetadata,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing report: {BlobUrl}", reportMetadata.BlobUrl);

        // Check if virus scan checking is enabled (disabled in dev with Azurite)
        var requireVirusScan = configuration.GetValue<bool>("Features:RequireVirusScan", false);

        if (requireVirusScan)
        {
            var scanStatus = await blobStorageService.GetVirusScanStatusAsync(
                reportMetadata.BlobUrl,
                cancellationToken);

            logger.LogInformation("Virus scan status for {BlobUrl}: {Status}", 
                reportMetadata.BlobUrl, scanStatus);

            switch (scanStatus)
            {
                case VirusScanStatus.Infected:
                    logger.LogError("Report is infected with malware: {BlobUrl}", reportMetadata.BlobUrl);
                    return new ProcessingResult(false, false, "File is infected with malware");

                case VirusScanStatus.Scanning:
                case VirusScanStatus.NotScanned:
                    logger.LogWarning("Report virus scan not complete: {BlobUrl}", reportMetadata.BlobUrl);
                    return new ProcessingResult(false, true, "Virus scan not complete");

                case VirusScanStatus.Error:
                    logger.LogError("Virus scan error for: {BlobUrl}", reportMetadata.BlobUrl);
                    return new ProcessingResult(false, true, "Virus scan error");

                case VirusScanStatus.Clean:
                    logger.LogInformation("Report passed virus scan: {BlobUrl}", reportMetadata.BlobUrl);
                    break;
            }
        }

        // TODO: Actual report processing logic
        logger.LogInformation(
            "Report processed successfully: {Organisation}/{Project}/{FileName} - {BlobUrl}",
            reportMetadata.Organisation,
            reportMetadata.Project,
            reportMetadata.FileName,
            reportMetadata.BlobUrl);

        return new ProcessingResult(true, false);
    }
}
