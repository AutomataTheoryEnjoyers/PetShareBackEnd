using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Pets;
using ShelterModule.Services.Interfaces.Announcements;

namespace ShelterModule.Services.Implementations.Announcements
{
    public class AnnoucementCommand : IAnnouncementCommand
    {
        private readonly PetShareDbContext _dbContext;

        public AnnoucementCommand(PetShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Announcement> AddAsync(Announcement announcement, CancellationToken token = default)
        {
            var entityAnnouncement = announcement.ToEntity();
            _dbContext.Add(entityAnnouncement);
            await _dbContext.SaveChangesAsync(token);
            return announcement;
        }

        public async Task<Announcement?> UpdateAsync(Guid id, AnnouncementPutRequest request, CancellationToken token = default)
        {
            var entityToUpdate = await _dbContext.Announcements.Include(x=>x.Pet).Include(x=>x.Author).FirstOrDefaultAsync(e => e.Id == id, token);
            if (entityToUpdate is null)
                return null;

            entityToUpdate.Title = request.Title ?? entityToUpdate.Title;
            entityToUpdate.Description = request.Description ?? entityToUpdate.Description;
            entityToUpdate.PetId = request.PetId ?? entityToUpdate.PetId;
            entityToUpdate.Status = request.Status ?? entityToUpdate.Status;

            if(entityToUpdate.Status == 1)//jesli status zostal zmieniony na closed to ustawiamy closingDate
                entityToUpdate.ClosingDate = DateTime.Now;
            entityToUpdate.LastUpdateDate = DateTime.Now;

            await _dbContext.SaveChangesAsync(token);
            return Announcement.FromEntity(entityToUpdate);
        }
    }
}
