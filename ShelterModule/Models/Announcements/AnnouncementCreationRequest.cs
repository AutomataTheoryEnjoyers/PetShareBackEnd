using System.ComponentModel.DataAnnotations;
using ShelterModule.Models.Pets;

namespace ShelterModule.Models.Announcements;

public sealed class AnnouncementCreationRequest
{
    [Required]
    public string Title { get; init; } = null!;

    [Required]
    public string Description { get; init; } = null!;

    [Required]
    public Guid ShelterId { get; init; }
    [Required]
    public Guid PetId { get; init; }

}
