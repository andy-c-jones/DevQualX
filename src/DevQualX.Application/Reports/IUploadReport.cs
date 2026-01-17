using DevQualX.Domain.Models;
using DevQualX.Functional;

namespace DevQualX.Application.Reports;

public interface IUploadReport
{
    Task<Result<ReportMetadata, Error>> ExecuteAsync(
        ReportUploadRequest request,
        CancellationToken cancellationToken = default);
}
