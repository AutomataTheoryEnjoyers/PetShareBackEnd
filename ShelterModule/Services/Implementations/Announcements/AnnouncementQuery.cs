﻿using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Announcements;
using ShelterModule.Services.Interfaces.Announcements;

namespace ShelterModule.Services.Implementations.Announcements;

public class AnnouncementQuery : IAnnouncementQuery
{
    private readonly PetShareDbContext _context;

    public AnnouncementQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Announcement>> GetAllFilteredAsync(GetAllAnnouncementsFilteredQueryRequest query,
        CancellationToken token = default)
    {
        var filteredAnnouncements = _context.Announcements.AsQueryable();

        if (query.Species is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => query.Species.Contains(a.Pet.Species));
        if (query.Breeds is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => query.Breeds.Contains(a.Pet.Breed));
        if (query.Cities is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => query.Cities.Contains(a.Pet.Shelter.Address.City));
        if (query.MinAge is not null)
            filteredAnnouncements =filteredAnnouncements.Where(a => EF.Functions.DateDiffYear(a.Pet.Birthday, DateTime.Now) >= query.MinAge);
        if (query.MaxAge is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => EF.Functions.DateDiffYear(a.Pet.Birthday, DateTime.Now) <= query.MaxAge);
        if (query.ShelterNames is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => query.ShelterNames.Contains(a.Pet.Shelter.FullShelterName));

        var entitiesList = await filteredAnnouncements.ToListAsync(token);
        return entitiesList.Select(Announcement.FromEntity).ToList();
    }

    public async Task<Announcement?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Announcements.Include(x => x.Author).
                                    Include(x => x.Pet).
                                    FirstOrDefaultAsync(e => e.Id == id, token);
        return entity is null ? null : Announcement.FromEntity(entity);
    }
}