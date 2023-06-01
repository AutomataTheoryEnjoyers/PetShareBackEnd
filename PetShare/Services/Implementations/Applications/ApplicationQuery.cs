using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Applications;
using PetShare.Services.Interfaces.Applications;

namespace PetShare.Services.Implementations.Applications;

public sealed class ApplicationQuery : IApplicationQuery
{
    private readonly PetShareDbContext _context;

    public ApplicationQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken token = default)
    {
        return await _context.Applications.Include(app => app.Announcement.Pet.Shelter).
                              Include(app => app.Adopter).
                              Select(app => Application.FromEntity(app)).
                              ToListAsync(token);
    }

    public async Task<IReadOnlyList<Application>?> GetAllForAdopterAsync(Guid adopterId,
        CancellationToken token = default)
    {
        if (!_context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).Any(adopter => adopter.Id == adopterId))
            return null;

        return await _context.Applications.Include(app => app.Announcement.Pet.Shelter).
                              Include(app => app.Adopter).
                              Where(app => app.Adopter.Id == adopterId).
                              Select(app => Application.FromEntity(app)).
                              ToListAsync(token);
    }

    public async Task<IReadOnlyList<Application>?> GetAllForShelterAsync(Guid shelterId,
        CancellationToken token = default)
    {
        if (!_context.Shelters.Any(shelter => shelter.Id == shelterId))
            return null;

        return await _context.Applications.Include(app => app.Announcement.Pet.Shelter).
                              Include(app => app.Adopter).
                              Where(app => app.Announcement.AuthorId == shelterId).
                              Select(app => Application.FromEntity(app)).
                              ToListAsync(token);
    }

    public async Task<Application?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Applications.Include(app => app.Announcement.Pet.Shelter).
                                    Include(app => app.Adopter).
                                    FirstOrDefaultAsync(app => app.Id == id, token);

        return entity is not null ? Application.FromEntity(entity) : null;
    }

    public async Task<IReadOnlyList<Application>?> GetAllForAnnouncementAsync(Guid announcementId, CancellationToken token = default)
    {
        if (!_context.Announcements.Any(announcement => announcement.Id == announcementId))
            return null;

        return await _context.Applications.
            Include(app => app.Announcement.Pet.Shelter).
            Include(app => app.Adopter).
            Where(app => app.AnnouncementId == announcementId).
            Select(app => Application.FromEntity(app)).
            ToListAsync(token);
    }
}
