using Database.Entities;
using Database.ValueObjects;

namespace ShelterModule.Models.Shelters;

public sealed class Shelter
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; } = null!;
    public required string FullShelterName { get; init; }
    public required string PhoneNumber { get; init; } = null!;
    public required string Email { get; init; } = null!;
    public required Address Address { get; init; } = null!;

    public required Address Address { get; init; } = null!;
    public required bool? IsAuthorized { get; set; } = null;
    // true - authorized 
    // false - blocked
    // null - unauthorized unblocked
    public required bool? IsAuthorized { get; set; }

    public ShelterEntity ToEntity()
    {
        return new ShelterEntity
        {
            Id = Id,
            UserName = UserName,
            FullShelterName = FullShelterName,
            PhoneNumber = PhoneNumber,
            Email = Email,
            IsAuthorized = IsAuthorized,
            Address = Address
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
            IsAuthorized = entity.IsAuthorized,
            Address = entity.Address
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
            IsAuthorized = IsAuthorized,
            Address = Address
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
            IsAuthorized = null,
            Address = request.Address
        };
    }
}
