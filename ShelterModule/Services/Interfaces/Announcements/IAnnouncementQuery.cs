using ShelterModule.Models.Announcements;

namespace ShelterModule.Services.Interfaces.Announcements;

public interface IAnnouncementQuery
{
    public Task<IReadOnlyList<Announcement>> GetAllFilteredAsync(GetAllAnnouncementsFilteredQueryRequest query,
        CancellationToken token = default);

    public Task<Announcement?> GetByIdAsync(Guid id, CancellationToken token = default);
}
