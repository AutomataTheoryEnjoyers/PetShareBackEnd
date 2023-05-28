using PetShare.Models.Announcements;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Announcements;

public interface IAnnouncementCommand
{
    public Task<Result> AddAsync(Announcement announcement, CancellationToken token = default);
    public Task<Announcement?> UpdateAsync(Guid id, AnnouncementPutRequest request, CancellationToken token = default);
}
