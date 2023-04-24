using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public sealed class ApplicationEntity
{
    [Key]
    public required Guid Id { get; init; }

    public required DateTime CreationTime { get; init; }

    public required ApplicationState State { get; set; }

    public required Guid AnnouncementId { get; init; }

    [Required]
    public AnnouncementEntity Announcement { get; init; } = null!;

    public required Guid AdopterId { get; init; }

    [Required]
    public AdopterEntity Adopter { get; init; } = null!;
}

public enum ApplicationState
{
    Submitted,
    Accepted,
    Rejected,
    Withdrawn
}
