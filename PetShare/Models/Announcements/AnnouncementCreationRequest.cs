using System.ComponentModel.DataAnnotations;

namespace PetShare.Models.Announcements;

public sealed class AnnouncementCreationRequest
{
    [Required]
    public string Title { get; init; } = null!;

    [Required]
    public string Description { get; init; } = null!;

    [Required]
    public Guid PetId { get; init; }
}
