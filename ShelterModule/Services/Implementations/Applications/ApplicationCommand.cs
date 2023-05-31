using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Adopters;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Applications;
using ShelterModule.Results;
using ShelterModule.Services.Interfaces.Applications;

namespace ShelterModule.Services.Implementations.Applications;

public sealed class ApplicationCommand : IApplicationCommand
{
    private readonly PetShareDbContext _context;

    public ApplicationCommand(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Application>> CreateAsync(Guid announcementId, Guid adopterId,
        CancellationToken token = default)
    {
        var adopter = await _context.Adopters.Where(adopter => adopter.Status != AdopterStatus.Deleted).
                                     FirstOrDefaultAsync(adopter => adopter.Id == adopterId, token);
        var announcement = await _context.Announcements.
                                          Where(announcement => announcement.Status != AnnouncementStatus.Deleted).
                                          FirstOrDefaultAsync(announcement => announcement.Id == announcementId, token);

        if (adopter is null)
            return new NotFound(adopterId, nameof(Adopter));
        if (announcement is null)
            return new NotFound(announcementId, nameof(Announcement));

        if (adopter.Status == AdopterStatus.Blocked)
            return new InvalidOperation("Adopter is blocked");

        if (announcement.Status == AnnouncementStatus.Closed)
            return new InvalidOperation("Announcement is closed");

        var id = Guid.NewGuid();
        var now = DateTime.Now;
        _context.Applications.Add(new ApplicationEntity
        {
            Id = id,
            CreationTime = now,
            LastUpdateTime = now,
            State = ApplicationState.Created,
            AdopterId = adopterId,
            AnnouncementId = announcementId
        });
        await _context.SaveChangesAsync(token);

        return Application.FromEntity(await _context.Applications.Include(app => app.Adopter).
                                                     Include(app => app.Announcement.Pet.Shelter).
                                                     FirstAsync(app => app.Id == id, token));
    }

    public async Task<Result<Application>> WithdrawAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Applications.Include(app => app.Announcement).
                                    Include(app => app.Adopter).
                                    FirstOrDefaultAsync(app => app.Id == id, token);

        if (entity is null)
            return new NotFound(id, nameof(Application));

        entity.State = ApplicationState.Withdrawn;
        entity.LastUpdateTime = DateTime.Now;
        await _context.SaveChangesAsync(token);

        return Application.FromEntity(entity);
    }

    public async Task<Result<Application>> AcceptAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Applications.Include(app => app.Announcement).
                                    Include(app => app.Adopter).
                                    FirstOrDefaultAsync(app => app.Id == id, token);

        if (entity is null)
            return new NotFound(id, nameof(Application));

        if (entity.State == ApplicationState.Withdrawn)
            return new InvalidOperation("Application was withdrawn");
        if (entity.State == ApplicationState.Rejected)
            return new InvalidOperation("Application was rejected");

        entity.State = ApplicationState.Accepted;
        entity.LastUpdateTime = DateTime.Now;
        await _context.SaveChangesAsync(token);

        return Application.FromEntity(entity);
    }

    public async Task<Result<Application>> RejectAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Applications.Include(app => app.Announcement).
                                    Include(app => app.Adopter).
                                    FirstOrDefaultAsync(app => app.Id == id, token);

        if (entity is null)
            return new NotFound(id, nameof(Application));

        if (entity.State == ApplicationState.Withdrawn)
            return new InvalidOperation("Application was withdrawn");
        if (entity.State == ApplicationState.Accepted)
            return new InvalidOperation("Application was accepted");

        entity.State = ApplicationState.Rejected;
        entity.LastUpdateTime = DateTime.Now;
        await _context.SaveChangesAsync(token);

        return Application.FromEntity(entity);
    }
}
