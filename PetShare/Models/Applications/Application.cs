using Database.Entities;
using PetShare.Models.Adopters;
using PetShare.Models.Announcements;

namespace PetShare.Models.Applications;

public sealed class Application
{
    public required Guid Id { get; init; }
    public required DateTime CreationTime { get; init; }
    public required DateTime LastUpdateTime { get; init; }
    public required ApplicationState State { get; init; }
    public required Announcement Announcement { get; init; }
    public required Adopter Adopter { get; init; }

    public ApplicationResponse ToResponse()
    {
        return new ApplicationResponse
        {
            Id = Id,
            CreationDate = CreationTime,
            LastUpdateDate = LastUpdateTime,
            ApplicationStatus = State.ToString().ToLower(),
            AnnouncementId = Announcement.Id,
            Announcement = Announcement.ToResponse(),
            Adopter = Adopter.ToResponse()
        };
    }

    public static Application FromEntity(ApplicationEntity entity)
    {
        return new Application
        {
            Id = entity.Id,
            State = entity.State,
            CreationTime = entity.CreationTime,
            LastUpdateTime = entity.LastUpdateTime,
            Adopter = Adopter.FromEntity(entity.Adopter),
            Announcement = Announcement.FromEntity(entity.Announcement)
        };
    }
}
