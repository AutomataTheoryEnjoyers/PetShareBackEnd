using ShelterModule.Models.Pets;
using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Announcements;

public sealed class AnnouncementResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required PetResponse Pet { get; init; }

    [Required]
    public required string Title { get; init; } = null!;

    [Required]
    public required string Description { get; init; } = null!;

    [Required]
    public required DateTime CreationDate { get; init; }

    public required DateTime? ClosingDate { get; init; }

    [Required]
    public required string Status { get; init; }

    [Required]
    public required DateTime LastUpdateDate { get; init; }
}
