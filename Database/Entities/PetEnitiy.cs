using System.ComponentModel.DataAnnotations;

namespace Database.Entities
{
    public class PetEnitiy
    {
        [Key]
        public required Guid Id { get; init; }

        // foreign key 
        public required ShelterEntity Shelter { get; init; } = null!;
        public required Guid ShelterId { get; init; }

        // fields
        public string Name { get; init; } = null!;
        public string Species { get; init; } = null!;
        public string Breed { get; init; } = null!;
        public DateTime Birthday { get; init; }
        public string Description { get; init; } = null!;
        
        //public byte[] Photo { get; init; } = null!; // ?

    }
}
