using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[PrimaryKey(nameof(AdopterId), nameof(AnnouncementId))]
public sealed class LikedAnnouncementEntity
{
    public required Guid AdopterId { get; init; }

    [Required]
    public AdopterEntity Adopter { get; init; } = null!;

    public required Guid AnnouncementId { get; init; }

    [Required]
    public AnnouncementEntity Announcement { get; init; } = null!;
}
