using PetShare.Models.Announcements;

namespace PetShare.Services.Interfaces.Announcements;

public interface IAnnouncementQuery
{
    Task<IReadOnlyList<Announcement>> GetAllFilteredAsync(GetAllAnnouncementsFilteredQueryRequest query,
        CancellationToken token = default);
    Task<IReadOnlyList<Announcement>> GetForShelterAsync(Guid shelterId, CancellationToken token = default);
    Task<Announcement?> GetByIdAsync(Guid id, CancellationToken token = default);
}
