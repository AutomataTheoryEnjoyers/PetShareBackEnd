using System.ComponentModel.DataAnnotations;

namespace PetShare.Models.Pets;

public sealed class PetResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required Guid ShelterId { get; init; }

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

    public string? PhotoUrl { get; init; }

    [Required]
    public required string Sex { get; init; }
}
