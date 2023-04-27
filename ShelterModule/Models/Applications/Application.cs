using Database.Entities;
using ShelterModule.Models.Adopters;
using ShelterModule.Models.Announcements;

namespace ShelterModule.Models.Applications;

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
        return new()
        {
            Id = Id,
            CreationTime = CreationTime,
            LastUpdateTime = LastUpdateTime,
            State = State.ToString(),
            AnnouncementId = Announcement.Id,
            Announcement = Announcement.ToResponse(),
            Adopter = Adopter.ToResponse()
        };
    }

    public static Application FromEntity(ApplicationEntity entity)
    {
        return new()
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
