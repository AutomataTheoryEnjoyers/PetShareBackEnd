using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Applications;

public sealed class ApplicationRequest
{
    [Required]
    public required Guid AnnouncementId { get; init; }
}
