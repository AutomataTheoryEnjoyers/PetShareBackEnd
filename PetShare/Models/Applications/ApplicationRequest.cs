using System.ComponentModel.DataAnnotations;

namespace PetShare.Models.Applications;

public sealed class ApplicationRequest
{
    [Required]
    public required Guid AnnouncementId { get; init; }
}
