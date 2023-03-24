using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Pets;

public sealed class PetUpsertRequest
{
    [Required]
    public string Name { get; init; } = null!;

    [Required]
    public required string Species { get; init; } = null!;

    [Required]
    public required string Breed { get; init; } = null!;

    [Required]
    public required DateTime Birthday { get; init; }

    [Required]
    public required string Description { get; init; } = null!;

    [Required]
    public required string Photo { get; init; } = null!;

    [Required]
    public required Guid ShelterId { get; init; }
}
