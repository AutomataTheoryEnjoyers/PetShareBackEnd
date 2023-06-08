using Database.Entities;
using PetShare.Models.Reports;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Reports;

public interface IReportCommand
{
    Task<Result> AddAsync(Report report, CancellationToken token = default);
    Task<Result<Report>> UpdateStateAsync(Guid id, ReportState newState, CancellationToken token = default);
}
