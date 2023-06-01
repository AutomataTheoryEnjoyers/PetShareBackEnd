using Database.Entities;
using PetShare.Models.Pets;

namespace PetShare.Models.Announcements;

public sealed class Announcement
{
    public Guid Id { get; init; }
    public required Guid AuthorId { get; init; }
    public required Pet Pet { get; init; }
    public required string Title { get; init; } = null!;
    public required string Description { get; init; } = null!;
    public required DateTime CreationDate { get; init; }
    public DateTime? ClosingDate { get; init; }
    public required AnnouncementStatus Status { get; init; }
    public required DateTime LastUpdateDate { get; init; }

    public AnnouncementEntity ToEntity()
    {
        return new AnnouncementEntity
        {
            Id = Id,
            AuthorId = AuthorId,
            PetId = Pet.Id,
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
            Pet = Pet.FromEntity(entity.Pet),
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
            Pet = Pet.ToResponse(),
            Title = Title,
            Description = Description,
            CreationDate = CreationDate,
            ClosingDate = ClosingDate,
            Status = Status.ToString(),
            LastUpdateDate = LastUpdateDate
        };
    }

    public static Announcement FromRequest(AnnouncementCreationRequest request, Guid shelterId, Pet pet)
    {
        return new Announcement
        {
            Id = Guid.NewGuid(),
            AuthorId = shelterId,
            Pet = pet,
            Title = request.Title,
            Description = request.Description,
            CreationDate = DateTime.Now,
            Status = AnnouncementStatus.Open,
            LastUpdateDate = DateTime.Now
        };
    }
}

public sealed record AnnouncementWithLike(Announcement Announcement, bool IsLiked)
{
    public LikedAnnouncementResponse ToResponse()
    {
        return new LikedAnnouncementResponse
        {
            Id = Announcement.Id,
            Pet = Announcement.Pet.ToResponse(),
            Title = Announcement.Title,
            Description = Announcement.Description,
            CreationDate = Announcement.CreationDate,
            ClosingDate = Announcement.ClosingDate,
            Status = Announcement.Status.ToString(),
            LastUpdateDate = Announcement.LastUpdateDate,
            IsLiked = IsLiked
        };
    }
}
