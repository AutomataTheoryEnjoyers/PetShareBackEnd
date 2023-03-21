using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Shelter;

public sealed class ShelterResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required string UserName { get; init; }

    [Required]
    public required string FullShelterName { get; init; }

    [Required]
    public required string Email { get; init; }

    [Required]
    public required string PhoneNumber { get; init; }

    [Required]
    public required bool? IsAuthorized { get; init; }
}
