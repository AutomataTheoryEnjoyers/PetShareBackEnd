using Database.Entities;

namespace PetShare.Models.Announcements;

public sealed class Announcement
{
    public Guid Id { get; init; }
    public required Guid AuthorId { get; init; }
    public required Guid PetId { get; init; }
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
            PetId = PetId,
            Title = Title,
            Description = Description,
            CreationDate = CreationDate,
            ClosingDate = ClosingDate,
            Status = (int)Status,
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
            Status = (AnnouncementStatus)entity.Status,
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
            Status = (int)Status,
            LastUpdateDate = LastUpdateDate
        };
    }

    public static Announcement FromRequest(AnnouncementCreationRequest request, Guid shelterId)
    {
        return new Announcement
        {
            Id = Guid.NewGuid(),
            AuthorId = shelterId,
            PetId = request.PetId,
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
            AuthorId = Announcement.AuthorId,
            PetId = Announcement.PetId,
            Title = Announcement.Title,
            Description = Announcement.Description,
            CreationDate = Announcement.CreationDate,
            ClosingDate = Announcement.ClosingDate,
            Status = (int)Announcement.Status,
            LastUpdateDate = Announcement.LastUpdateDate,
            IsLiked = IsLiked
        };
    }
}

public enum AnnouncementStatus
{
    Open = 0,
    Closed = 1,
    DuringVerification = 2,
    Deleted = 3
}
