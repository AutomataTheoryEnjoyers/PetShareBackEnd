using Database.Entities;

namespace ShelterModule.Models.Pets;

public class Pet
{
    public Guid Id { get; init; }
    public required Guid ShelterId { get; init; }
    public required string Name { get; init; } = null!;
    public required string Species { get; init; } = null!;
    public required string Breed { get; init; } = null!;
    public required DateTime Birthday { get; init; }
    public required string Description { get; init; } = null!;
    public required string Photo { get; init; } = null!;

    public PetEntity ToEntity()
    {
        return new PetEntity
        {
            Id = Id,
            Name = Name,
            Species = Species,
            Breed = Breed,
            Birthday = Birthday,
            Description = Description,
            Photo = Photo,
            ShelterId = ShelterId
        };
    }

    public static Pet FromEntity(PetEntity entity)
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
            ShelterId = entity.ShelterId
        };
    }

    public static Pet FromRequest(PetUpsertRequest request)
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
            ShelterId = request.ShelterId
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
            ShelterId = ShelterId
        };
    }
}
