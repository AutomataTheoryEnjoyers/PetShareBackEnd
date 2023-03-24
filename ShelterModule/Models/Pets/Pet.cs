using Database.Entities;
using ShelterModule.Models.Shelters;

namespace ShelterModule.Models.Pets
{
    public class Pet
    {
        public Guid Id { get; init; }
        public required Shelter Shelter { get; init; } = null!;
        public required string Name { get; init; } = null!;
        public required string Species { get; init; } = null!;
        public required string Breed { get; init; } = null!;
        public required DateTime Birthday { get; init; }
        public required string Description { get; init; } = null!;
        public required string Photo { get; init; } = null!;

        public PetEnitiy ToEntity()
        {
            return new PetEnitiy
            {
                Id = Id,
                Name = Name,
                Species = Species,
                Breed = Breed,
                Birthday = Birthday,
                Description = Description,
                Photo = Photo,
                ShelterId = Shelter.Id
            };
        }

        public static Pet FromEntity(PetEnitiy entity)
        {
            return new Pet
            {
                Id = entity.Id,
                Name = entity.Name,
                Species = entity.Species,
                Breed = entity.Breed,
                Birthday = entity.Birthday,
                Description = entity.Description,
                Photo = entity.Photo,
                Shelter = Shelter.FromEntity(entity.Shelter)
            };
        }

        public static Pet FromRequest(PetCreationRequest request, Shelter shelter)
        {
            return new Pet
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Species = request.Species,
                Breed = request.Breed,
                Birthday = request.Birthday,
                Description = request.Description,
                Photo = request.Photo,
                Shelter = shelter
            };
        }

        public PetResponse ToResponse()
        {
            return new PetResponse
            {
                Id = Id,
                Name = Name,
                Species = Species,
                Breed = Breed,
                Birthday = Birthday,
                Description = Description,
                Photo = Photo,
                ShelterId = Shelter.Id
            };
        }

    }
}
