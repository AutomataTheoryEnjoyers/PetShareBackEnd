﻿using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Applications;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Applications;

namespace ShelterModule.Services.Implementations.Applications;

public sealed class ApplicationQuery : IApplicationQuery
{
    private readonly PetShareDbContext _context;

    public ApplicationQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken token = default)
    {
        return await _context.Applications.Include(app => app.Announcement).
                              Include(app => app.Adopter).
                              Select(app => Application.FromEntity(app)).
                              ToListAsync(token);
    }

    public async Task<IReadOnlyList<Application>?> GetAllForAdopterAsync(Guid adopterId,
        CancellationToken token = default)
    {
        if (!_context.Adopters.Any(adopter => adopter.Id == adopterId))
            return null;

        return await _context.Applications.Include(app => app.Announcement).
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

        return await _context.Applications.Include(app => app.Announcement).
                              Include(app => app.Adopter).
                              Where(app => app.Announcement.AuthorId == shelterId).
                              Select(app => Application.FromEntity(app)).
                              ToListAsync(token);
    }

    public async Task<Application?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Applications.Include(app => app.Announcement).
                                    Include(app => app.Adopter).
                                    FirstOrDefaultAsync(app => app.Id == id, token);

        return entity is not null ? Application.FromEntity(entity) : null;
    }

    public async Task<IReadOnlyList<Application>?> GetByAnnouncementId(Guid announecemetID, CancellationToken token = default)
    {
        if (!_context.Announcements.Any(announcement => announcement.Id == announecemetID))
            return null;

        return await _context.Applications.Include(app => app.Announcement).
                              Include(app => app.Adopter).
                              Where(app => app.Announcement.Id == announecemetID).
                              Select(app => Application.FromEntity(app)).
                              ToListAsync(token);
    }
}
