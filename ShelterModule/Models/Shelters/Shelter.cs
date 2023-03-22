using Database.Entities;

namespace ShelterModule.Models.Shelters;

public sealed class Shelter : User
{
    public required bool? IsAuthorized { get; set; }
    // true - authorized 
    // false - blocked
    // null - unauthorized unblocked

    public required string FullShelterName { get; init; }

    // list<Pet> ? 

    public ShelterEntity ToEntity()
    {
        return new ShelterEntity
        {
            Id = Id,
            UserName = UserName,
            FullShelterName = FullShelterName,
            PhoneNumber = PhoneNumber,
            Email = Email,
            IsAuthorized = IsAuthorized
        };
    }

    public static Shelter FromEntity(ShelterEntity entity)
    {
        return new Shelter
        {
            Id = entity.Id,
            UserName = entity.UserName,
            FullShelterName = entity.FullShelterName,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            IsAuthorized = entity.IsAuthorized
        };
    }

    public ShelterResponse ToResponse()
    {
        return new ShelterResponse
        {
            Id = Id,
            UserName = UserName,
            FullShelterName = FullShelterName,
            PhoneNumber = PhoneNumber,
            Email = Email,
            IsAuthorized = IsAuthorized
        };
    }

    public static Shelter FromRequest(ShelterCreationRequest request)
    {
        return new Shelter
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            FullShelterName = request.FullShelterName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            IsAuthorized = null
        };
    }
}
