namespace ShelterModule.Models.Pet
{
    public class Pet
    {
        public Guid Id { get; init; }
        public required string Name { get; init; } = null!;
        public required string Species { get; init; } = null!;
        public required string Breed { get; init; } = null!;
        public required DateTime Birthday { get; init; }
        public required string Description { get; init; } = null!;
        public byte[] Photo { get; init; } = null!; // ?

    }
}
