using ShelterModule.Models.Shelters;

namespace ShelterModule.Models.Pets;

public class Pet
{
    public required Guid Id { get; init; }
    public required Shelter Shelter { get; init; } = null!;
    public required string Name { get; init; } = null!;
    public required string Species { get; init; } = null!;
    public required string Breed { get; init; } = null!;
    public required DateTime Birthday { get; init; }
    public required string Description { get; init; } = null!;
    public byte[] Photo { get; init; } = null!; // ?
}
