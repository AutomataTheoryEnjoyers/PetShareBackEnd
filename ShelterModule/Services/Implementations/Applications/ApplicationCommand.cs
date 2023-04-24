using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Applications;
using ShelterModule.Services.Interfaces.Applications;

namespace ShelterModule.Services.Implementations.Applications;

public sealed class ApplicationCommand : IApplicationCommand
{
    private readonly PetShareDbContext _context;

    public ApplicationCommand(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<Application?> CreateAsync(Guid announcementId, Guid adopterId, CancellationToken token = default)
    {
        if (!_context.Adopters.Where(adopter => adopter.Status != AdopterStatus.Deleted).
                      Any(adopter => adopter.Id == adopterId)
            || !_context.Announcements.Where(announcement => announcement.Status != (int)AnnouncementStatus.Deleted).
                         Any(announcement => announcement.Id == announcementId))
            return null;

        var id = Guid.NewGuid();
        _context.Applications.Add(new ApplicationEntity
        {
            Id = id,
            CreationTime = DateTime.Now,
            State = ApplicationState.Submitted,
            AdopterId = adopterId,
            AnnouncementId = announcementId
        });
        await _context.SaveChangesAsync(token);

        return Application.FromEntity(await _context.Applications.Include(app => app.Adopter).
                                                     Include(app => app.Announcement).
                                                     FirstAsync(app => app.Id == id, token));
    }

    public Task<Application?> WithdrawAsync(Guid applicationId, CancellationToken token = default)
    {
        return SetStateAsync(applicationId, ApplicationState.Withdrawn, token);
    }

    public Task<Application?> AcceptAsync(Guid applicationId, CancellationToken token = default)
    {
        return SetStateAsync(applicationId, ApplicationState.Accepted, token);
    }

    public Task<Application?> RejectAsync(Guid applicationId, CancellationToken token = default)
    {
        return SetStateAsync(applicationId, ApplicationState.Rejected, token);
    }

    private async Task<Application?> SetStateAsync(Guid id, ApplicationState state, CancellationToken token = default)
    {
        var entity = await _context.Applications.Include(app => app.Announcement).
                                    Include(app => app.Adopter).
                                    FirstOrDefaultAsync(app => app.Id == id, token);

        if (entity is null)
            return null;

        entity.State = state;
        await _context.SaveChangesAsync(token);

        return Application.FromEntity(entity);
    }
}
