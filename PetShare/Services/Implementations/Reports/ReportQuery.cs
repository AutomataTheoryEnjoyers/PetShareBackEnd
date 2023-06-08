using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Reports;
using PetShare.Services.Interfaces.Reports;

namespace PetShare.Services.Implementations.Reports;

public sealed class ReportQuery : IReportQuery
{
    private readonly PetShareDbContext _context;

    public ReportQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Report>> GetNewReportsAsync(CancellationToken token = default)
    {
        return await _context.Reports.Where(report => report.State == ReportState.New).
                              OrderByDescending(report => report.CreationTime).
                              Select(report => Report.FromEntity(report)).
                              ToListAsync(token);
    }
}
