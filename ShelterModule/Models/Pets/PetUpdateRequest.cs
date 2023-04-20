namespace ShelterModule.Models.Pets;

public sealed class PetUpdateRequest
{
    public required string? Name { get; init; }

    public required string? Species { get; init; }

    public required string? Breed { get; init; }

    public required DateTime? Birthday { get; init; }

    public required string? Description { get; init; }
}
