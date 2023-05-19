using System.ComponentModel.DataAnnotations;
using PetShare.Models.Adopters;
using PetShare.Models.Announcements;

namespace PetShare.Models.Applications;

public sealed class ApplicationResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required DateTime CreationTime { get; init; }

    [Required]
    public required DateTime LastUpdateTime { get; init; }

    [Required]
    public required string State { get; init; }

    [Required]
    public required Guid AnnouncementId { get; init; }

    [Required]
    public required AnnouncementResponse Announcement { get; init; }

    [Required]
    public required AdopterResponse Adopter { get; init; }
}
