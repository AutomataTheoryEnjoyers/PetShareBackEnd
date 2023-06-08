using PetShare.Models.Reports;

namespace PetShare.Services.Interfaces.Reports;

public interface IReportQuery
{
    Task<IReadOnlyList<Report>> GetNewReportsAsync(CancellationToken token = default);
}
