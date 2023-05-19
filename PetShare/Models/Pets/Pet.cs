using Database.Entities;

namespace PetShare.Models.Pets;

public class Pet
{
    public Guid Id { get; init; }
    public required Guid ShelterId { get; init; }
    public required string Name { get; init; }
    public required string Species { get; init; }
    public required string Breed { get; init; }
    public required DateTime Birthday { get; init; }
    public required string Description { get; init; }
    public string? Photo { get; init; }
    public required PetSex Sex { get; init; }
    public required PetStatus Status { get; init; }

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
            ShelterId = ShelterId,
            Sex = Sex,
            Status = Status
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
            ShelterId = entity.ShelterId,
            Sex = entity.Sex,
            Status = entity.Status
        };
    }

    public static Pet FromRequest(PetCreationRequest request, Guid shelterId)
    {
        return new Pet
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            Birthday = request.Birthday,
            Description = request.Description,
            Photo = null,
            ShelterId = shelterId,
            Sex = Enum.Parse<PetSex>(request.Sex),
            Status = PetStatus.Active
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
            PhotoUrl = Photo,
            ShelterId = ShelterId,
            Sex = Sex.ToString()
        };
    }
}
