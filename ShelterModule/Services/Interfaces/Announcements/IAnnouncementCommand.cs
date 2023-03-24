using ShelterModule.Models.Announcements;

namespace ShelterModule.Services.Interfaces.Announcements
{
    public interface IAnnouncementCommand
    {
        public Task<Announcement> AddAsync(Announcement announcement);
        public Task RemoveAsync(Announcement announcement);
        public Task<Announcement?> UpdateAsync(Guid id, AnnouncementPutRequest request);
    }
}
