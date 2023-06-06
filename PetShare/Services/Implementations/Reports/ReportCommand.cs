using System.Diagnostics;
using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Reports;
using PetShare.Results;
using PetShare.Services.Interfaces.Reports;

namespace PetShare.Services.Implementations.Reports;

public sealed class ReportCommand : IReportCommand
{
    private readonly PetShareDbContext _context;

    public ReportCommand(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<Result> AddAsync(Report report, CancellationToken token = default)
    {
        var existsResult = report.TargetType switch
        {
            ReportedEntityType.Adopter => await CheckAdopterAsync(report.TargetId, token),
            ReportedEntityType.Announcement => await CheckAnnouncementAsync(report.TargetId, token),
            ReportedEntityType.Shelter => await CheckShelterAsync(report.TargetId, token),
            _ => throw new UnreachableException()
        };
        if (!existsResult.HasValue)
            return existsResult.State;

        _context.Reports.Add(report.ToEntity());
        await _context.SaveChangesAsync(token);
        return Result.Ok;
    }

    public async Task<Result<Report>> UpdateStateAsync(Guid id, ReportState newState, CancellationToken token = default)
    {
        var entity = await _context.Reports.FirstOrDefaultAsync(report => report.Id == id, token);
        if (entity is null)
            return new ReportNotFound(id);

        entity.State = newState;
        await _context.SaveChangesAsync(token);

        return Report.FromEntity(entity);
    }

    private async Task<Result> CheckAdopterAsync(Guid id, CancellationToken token)
    {
        return await _context.Adopters.AnyAsync(adopter => adopter.Id == id, token)
            ? Result.Ok
            : new AdopterNotFound(id);
    }

    private async Task<Result> CheckShelterAsync(Guid id, CancellationToken token)
    {
        return await _context.Shelters.AnyAsync(shelter => shelter.Id == id, token)
            ? Result.Ok
            : new ShelterNotFound(id);
    }

    private async Task<Result> CheckAnnouncementAsync(Guid id, CancellationToken token)
    {
        return await _context.Shelters.AnyAsync(announcement => announcement.Id == id, token)
            ? Result.Ok
            : new AnnouncementNotFound(id);
    }
}
