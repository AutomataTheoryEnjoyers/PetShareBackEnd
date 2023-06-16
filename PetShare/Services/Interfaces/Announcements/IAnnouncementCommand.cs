using PetShare.Models.Announcements;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Announcements;

public interface IAnnouncementCommand
{
    Task<Result> AddAsync(Announcement announcement, CancellationToken token = default);
    Task<Announcement?> UpdateAsync(Guid id, AnnouncementPutRequest request, CancellationToken token = default);
    Task<Result> LikeAsync(Guid id, Guid adopterId, bool isLiked, CancellationToken token = default);
}
