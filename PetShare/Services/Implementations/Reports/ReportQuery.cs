using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Reports;
using PetShare.Results;
using PetShare.Services.Interfaces.Reports;

namespace PetShare.Services.Implementations.Reports;

public sealed class ReportQuery : IReportQuery
{
    private readonly PetShareDbContext _context;

    public ReportQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReportPage>> GetNewReportsPageAsync(int pageNumber, int pageSize,
        CancellationToken token = default)
    {
        if (pageNumber < 0)
            return new InvalidOperation("Page number must be non-negative");
        if (pageSize <= 0)
            return new InvalidOperation("Page size must be positive");

        var count = await _context.Reports.CountAsync(report => report.State == ReportState.New, token);
        var reports = await _context.Reports.Where(report => report.State == ReportState.New).
                                     OrderByDescending(report => report.CreationTime).
                                     Skip(pageNumber * pageSize).
                                     Take(pageSize).
                                     Select(report => Report.FromEntity(report)).
                                     ToListAsync(token);

        return new ReportPage(reports, pageNumber, count);
    }
}
