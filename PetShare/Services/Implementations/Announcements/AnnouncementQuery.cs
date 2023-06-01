using Database;
using Database.Entities;
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

    public async Task<IReadOnlyList<AnnouncementWithLike>> GetAllFilteredAsync(AnnouncementFilters filters,
        CancellationToken token = default)
    {
        var filteredAnnouncements = _context.Announcements.Include(a => a.Pet.Shelter).
                                             Where(a => a.Status == AnnouncementStatus.Open).
                                             AsQueryable();

        if (filters.Species is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => filters.Species.Contains(a.Pet.Species));
        if (filters.Breeds is not null)
            filteredAnnouncements = filteredAnnouncements.Where(a => filters.Breeds.Contains(a.Pet.Breed));
        if (filters.Cities is not null)
            filteredAnnouncements =
                filteredAnnouncements.Where(a => filters.Cities.Contains(a.Pet.Shelter.Address.City));
        if (filters.MinAge is not null)
            filteredAnnouncements =
                filteredAnnouncements.Where(a => EF.Functions.DateDiffYear(a.Pet.Birthday, DateTime.Now)
                                                 >= filters.MinAge);
        if (filters.MaxAge is not null)
            filteredAnnouncements =
                filteredAnnouncements.Where(a => EF.Functions.DateDiffYear(a.Pet.Birthday, DateTime.Now)
                                                 <= filters.MaxAge);
        if (filters.ShelterNames is not null)
            filteredAnnouncements =
                filteredAnnouncements.Where(a => filters.ShelterNames.Contains(a.Pet.Shelter.FullShelterName));

        var announcementsWithLikes = filters.MarkLikedBy is { } adopterId
            ? filteredAnnouncements.Select(a => new
            {
                Entity = a,
                Liked = _context.Likes.Any(l => l.AdopterId == adopterId && l.AnnouncementId == a.Id)
            })
            : filteredAnnouncements.Select(a => new { Entity = a, Liked = false });

        if (filters.IncludeOnlyLiked)
            announcementsWithLikes = announcementsWithLikes.Where(a => a.Liked);

        return await announcementsWithLikes.
                     Select(e => new AnnouncementWithLike(Announcement.FromEntity(e.Entity), e.Liked)).
                     ToListAsync(token);
    }

    public async Task<IReadOnlyList<Announcement>> GetForShelterAsync(Guid shelterId, CancellationToken token = default)
    {
        return (await _context.Announcements.Where(a => a.Status != AnnouncementStatus.Deleted).
                               Where(a => a.AuthorId == shelterId).
                               Include(a => a.Pet.Shelter).
                               ToListAsync(token)).
               Select(Announcement.FromEntity).
               ToList();
    }

    public async Task<Announcement?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Announcements.Where(a => a.Status != AnnouncementStatus.Deleted).
                                    Include(x => x.Author).
                                    Include(x => x.Pet).
                                    FirstOrDefaultAsync(e => e.Id == id, token);
        return entity is null ? null : Announcement.FromEntity(entity);
    }
}
