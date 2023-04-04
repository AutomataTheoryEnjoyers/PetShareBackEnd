using Database.Entities;

namespace ShelterModule.Models.Announcements;

public sealed class Announcement
{
    public Guid Id { get; init; }
    public required Guid AuthorId { get; init; }
    public required Guid PetId { get; init; }
    public required string Title { get; init; } = null!;
    public required string Description { get; init; } = null!;
    public required DateTime CreationDate { get; init; }
    public DateTime? ClosingDate { get; init; }
    public required int Status { get; init; }
    public required DateTime LastUpdateDate { get; init; }

    public AnnouncementEntity ToEntity()
    {
        return new AnnouncementEntity
        {
            Id = Id,
            AuthorId = AuthorId,
            PetId = PetId,
            Title = Title,
            Description = Description,
            CreationDate = CreationDate,
            ClosingDate = ClosingDate,
            Status = Status,
            LastUpdateDate = LastUpdateDate
        };
    }

    public static Announcement FromEntity(AnnouncementEntity entity)
    {
        return new Announcement
        {
            Id = entity.Id,
            AuthorId = entity.AuthorId,
            PetId = entity.PetId,
            Title = entity.Title,
            Description = entity.Description,
            CreationDate = entity.CreationDate,
            ClosingDate = entity.ClosingDate,
            Status = entity.Status,
            LastUpdateDate = entity.LastUpdateDate
        };
    }

    public AnnouncementResponse ToResponse()
    {
        return new AnnouncementResponse
        {
            Id = Id,
            AuthorId = AuthorId,
            PetId = PetId,
            Title = Title,
            Description = Description,
            CreationDate = CreationDate,
            ClosingDate = ClosingDate,
            Status = Status,
            LastUpdateDate = LastUpdateDate
        };
    }

    public static Announcement FromRequest(AnnouncementCreationRequest request, Guid petId)
    {
        return new Announcement
        {
            Id = Guid.NewGuid(),
            AuthorId = request.ShelterId,
            PetId = petId,
            Title = request.Title,
            Description = request.Description,
            CreationDate = DateTime.Now,
            Status = 0,
            LastUpdateDate = DateTime.Now
        };
    }
}
