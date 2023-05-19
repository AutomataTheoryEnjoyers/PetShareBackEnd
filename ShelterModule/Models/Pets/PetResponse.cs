using Database.Entities;
using ShelterModule.Models.Shelters;
using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Pets;

public sealed class PetResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required ShelterResponse Shelter { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Species { get; init; }

    public string? Breed { get; init; }

    [Required]
    public required DateTime Birthday { get; init; }

    public string? Description { get; init; }

    [Required]
    public required string PhotoUrl { get; init; }

    [Required]
    public required PetStatus Status { get; init; }

    [Required]
    public required string Sex { get; init; }
}
