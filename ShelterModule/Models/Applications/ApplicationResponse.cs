using System.ComponentModel.DataAnnotations;
using Database.Entities;
using ShelterModule.Models.Adopters;

namespace ShelterModule.Models.Applications;

public sealed class ApplicationResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required DateTime CreationTime { get; init; }

    [Required]
    public required ApplicationState State { get; init; }

    [Required]
    public required Guid AnnouncementId { get; init; }

    [Required]
    public required AdopterResponse Adopter { get; init; }
}
