using PetShare.Models.Announcements;

namespace PetShare.Services.Interfaces.Announcements;

public interface IAnnouncementQuery
{
    public Task<IReadOnlyList<AnnouncementWithLike>> GetAllFilteredAsync(AnnouncementFilters filters,
        CancellationToken token = default);

    public Task<IReadOnlyList<Announcement>> GetForShelterAsync(Guid shelterId, CancellationToken token = default);

    public Task<Announcement?> GetByIdAsync(Guid id, CancellationToken token = default);
}
