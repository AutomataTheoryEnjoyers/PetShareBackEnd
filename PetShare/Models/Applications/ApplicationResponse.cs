using System.ComponentModel.DataAnnotations;
using PetShare.Models.Adopters;
using PetShare.Models.Announcements;

namespace PetShare.Models.Applications;

public sealed class ApplicationResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required DateTime CreationDate { get; init; }

    [Required]
    public required DateTime LastUpdateDate { get; init; }

    [Required]
    public required string ApplicationStatus { get; init; }

    [Required]
    public required Guid AnnouncementId { get; init; }

    [Required]
    public required AnnouncementResponse Announcement { get; init; }

    [Required]
    public required AdopterResponse Adopter { get; init; }
}
