using System.ComponentModel.DataAnnotations;

namespace Database.Entities
{
    public sealed class PetEnitiy
    {
        [Key]
        public required Guid Id { get; init; }

        [Required]
        public ShelterEntity Shelter { get; init; } = null!;
        public required Guid ShelterId { get; init; }

        public string Name { get; init; } = null!;
        public string Species { get; init; } = null!;
        public string Breed { get; init; } = null!;
        public DateTime Birthday { get; init; }
        public string Description { get; init; } = null!;
        public string Photo { get; init; } = null!;
    }
}
