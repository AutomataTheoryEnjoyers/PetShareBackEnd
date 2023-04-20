using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Pets;

public sealed class PetCreationRequest
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Species { get; init; }

    [Required]
    public required string Breed { get; init; }

    [Required]
    public required DateTime Birthday { get; init; }

    [Required]
    public required string Description { get; init; }
}
