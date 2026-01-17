using DevQualX.Domain.Models;

namespace DevQualX.Application.Reports;

public interface IProcessReport
{
    Task<ProcessingResult> ExecuteAsync(
        ReportMetadata reportMetadata,
        CancellationToken cancellationToken = default);
}
