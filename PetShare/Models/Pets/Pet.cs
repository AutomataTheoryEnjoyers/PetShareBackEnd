using Database.Entities;
using PetShare.Models.Shelters;

namespace PetShare.Models.Pets;

public class Pet
{
    private const string DefaultPhotoUrl = "https://cdn.onlinewebfonts.com/svg/img_233291.png";

    public Guid Id { get; init; }
    public required Shelter Shelter { get; init; }
    public required string Name { get; init; }
    public required string Species { get; init; }
    public required string Breed { get; init; }
    public required DateTime Birthday { get; init; }
    public required string Description { get; init; }
    public required string Photo { get; init; }
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
            ShelterId = Shelter.Id,
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
            Shelter = Shelter.FromEntity(entity.Shelter),
            Sex = entity.Sex,
            Status = entity.Status
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
            Photo = request.PhotoUrl ?? DefaultPhotoUrl,
            Shelter = shelter,
            Sex = Enum.Parse<PetSex>(char.ToUpper(request.Sex[0]) + request.Sex.Substring(1)),
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
            Shelter = Shelter.ToResponse(),
            Sex = Sex.ToString(),
            Status = Status.ToString()
        };
    }
}
