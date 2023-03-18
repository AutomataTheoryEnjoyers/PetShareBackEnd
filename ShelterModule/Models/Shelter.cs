using Database.Entities;

namespace ShelterModule.Models;

public sealed class Shelter
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required bool IsAuthorized { get; init; }

    public ShelterEntity ToEntity()
    {
        return new ShelterEntity
        {
            Id = Id,
            Name = Name,
            IsAuthorized = IsAuthorized
        };
    }

    public static Shelter FromEntity(ShelterEntity entity)
    {
        return new Shelter
        {
            Id = entity.Id,
            Name = entity.Name,
            IsAuthorized = entity.IsAuthorized
        };
    }

    public ShelterResponse ToResponse()
    {
        return new ShelterResponse
        {
            Id = Id,
            Name = Name,
            IsAuthorized = IsAuthorized
        };
    }

    public static Shelter FromRequest(ShelterCreationRequest request)
    {
        return new Shelter
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            IsAuthorized = false
        };
    }
}
