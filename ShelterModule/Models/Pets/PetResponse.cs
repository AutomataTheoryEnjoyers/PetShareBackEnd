using ShelterModule.Models.Shelters;
using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Pets
{
    public sealed class PetResponse
    {
        public required Guid Id { get; init; }

        public required Guid ShelterId { get; init; }
        public required string Name { get; init; }
        public required string Species { get; init; }
        public required string Breed { get; init; }
        public required DateTime Birthday { get; init; }
        public required string Description { get; init; }
        public required string Photo { get; init; }
    }
}
