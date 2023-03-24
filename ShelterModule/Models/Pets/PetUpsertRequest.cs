using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Pets
{
    public sealed class PetUpsertRequest
    {
        [Required]
        [MaxLength(20)]
        public string Name { get; init; } = null!;
        
        [Required]
        [MaxLength(20)]
        public required string Species { get; init; } = null!;
        
        [Required]
        [MaxLength(20)]
        public required string Breed { get; init; } = null!;
        public required DateTime Birthday { get; init; }

        [Required]
        [MaxLength(150)]
        public required string Description { get; init; } = null!;

        [Required]
        public required string Photo { get; init; } = null!;

        [Required]
        public required Guid ShelterId { get; init; }
    }
}
