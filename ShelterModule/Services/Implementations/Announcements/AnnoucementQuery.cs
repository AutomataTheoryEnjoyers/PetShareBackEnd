using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Announcements;

namespace ShelterModule.Services.Implementations.Announcements
{
    public class AnnoucementQuery : IAnnouncementQuery
    {
        private readonly PetShareDbContext _context;

        public AnnoucementQuery(PetShareDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Announcement>> GetAllFilteredAsync(GetAllAnnouncementsFilteredQuery query, CancellationToken token = default)
        {
            var filteredAnnouncements = (await _context.Announcements.Include(x => x.Author).Include(x => x.Pet).ToListAsync(token)).
                Select(a=>new { Announcement = Announcement.FromEntity(a)
                ,Pet = new { Pet = Pet.FromEntity(a.Pet), Shelter = Shelter.FromEntity(a.Pet.Shelter) }
                ,Author = Shelter.FromEntity(a.Author)});

            if (query.Species is not null)
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => query.Species.Contains(a.Pet.Pet.Species));
            }
            if (query.Breeds is not null)
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => query.Breeds.Contains(a.Pet.Pet.Breed));
            }
            if (query.Cities is not null)
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => query.Cities.Contains(a.Pet.Shelter.Address.City));
            }
            if (query.MinAge is not null) 
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => (DateTime.Now.Year - a.Pet.Pet.Birthday.Year) >= query.MinAge);
            }
            if (query.MaxAge is not null) 
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => (DateTime.Now.Year - a.Pet.Pet.Birthday.Year) <= query.MaxAge);
            }
            if (query.ShelterNames is not null)
            {
                filteredAnnouncements = filteredAnnouncements.Where(a => query.ShelterNames.Contains(a.Pet.Shelter.FullShelterName));
            }

            return filteredAnnouncements.Select(a=>a.Announcement).ToList();
        }

        public async Task<Announcement?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            var entity = await _context.Announcements.Include(x => x.Author).Include(x=>x.Pet).FirstOrDefaultAsync(e => e.Id == id, token);
            return entity is null ? null : Announcement.FromEntity(entity);
        }
    }
}
