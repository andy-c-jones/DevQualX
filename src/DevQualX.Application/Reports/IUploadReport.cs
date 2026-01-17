using DevQualX.Domain.Models;

namespace DevQualX.Application.Reports;

public interface IUploadReport
{
    Task<ReportMetadata> ExecuteAsync(
        ReportUploadRequest request,
        CancellationToken cancellationToken = default);
}
