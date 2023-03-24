using ShelterModule.Models.Announcements;
using ShelterModule.Models.Shelters;

namespace ShelterModule.Services.Interfaces.Announcements
{
    public interface IAnnouncementQuery
    {
        public Task<IReadOnlyList<Announcement>> GetAllFilteredAsync(GetAllAnnouncementsFilteredQuery query, CancellationToken token = default);
        public Task<Announcement?> GetByIdAsync(Guid id, CancellationToken token = default);
    }
}
