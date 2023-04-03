using ShelterModule.Models.Announcements;

namespace ShelterModule.Services.Interfaces.Announcements
{
    public interface IAnnouncementCommand
    {
        public Task<Announcement> AddAsync(Announcement announcement, CancellationToken token = default);
        public Task<Announcement?> UpdateAsync(Guid id, AnnouncementPutRequest request, CancellationToken token = default);
    }
}
