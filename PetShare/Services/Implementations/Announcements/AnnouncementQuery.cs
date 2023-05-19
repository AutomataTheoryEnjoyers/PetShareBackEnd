using Database;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Announcements;
using PetShare.Services.Interfaces.Announcements;

namespace PetShare.Services.Implementations.Announcements;

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
        var filteredAnnouncements = _context.Announcements.Where(a => a.Status == (int)AnnouncementStatus.Open).
                                             Include(x => x.Pet.Shelter).
                                             AsQueryable();

        if (query.Species is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => query.Species.Contains(a.Pet.Species));
        if (query.Breeds is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => query.Breeds.Contains(a.Pet.Breed));
        if (query.Cities is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => query.Cities.Contains(a.Pet.Shelter.Address.City));
        if (query.MinAge is not null)
            filteredAnnouncements =
                filteredAnnouncements.Where(a => EF.Functions.DateDiffYear(a.Pet.Birthday, DateTime.Now)
                                                 >= query.MinAge);
        if (query.MaxAge is not null)
            filteredAnnouncements =
                filteredAnnouncements.Where(a => EF.Functions.DateDiffYear(a.Pet.Birthday, DateTime.Now)
                                                 <= query.MaxAge);
        if (query.ShelterNames is not null)
            filteredAnnouncements =
                filteredAnnouncements.Where(a => query.ShelterNames.Contains(a.Pet.Shelter.FullShelterName));

        return await filteredAnnouncements.Select(e => Announcement.FromEntity(e)).ToListAsync(token);
    }

    public async Task<IReadOnlyList<Announcement>> GetForShelterAsync(Guid shelterId, CancellationToken token = default)
    {
        return (await _context.Announcements.Where(a => a.AuthorId == shelterId).Include(a => a.Pet.Shelter).ToListAsync(token)).
               Select(Announcement.FromEntity).
               ToList();
    }

    public async Task<Announcement?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Announcements.Include(x => x.Author).
                                    Include(x => x.Pet).
                                    FirstOrDefaultAsync(e => e.Id == id, token);
        return entity is null ? null : Announcement.FromEntity(entity);
    }
}
