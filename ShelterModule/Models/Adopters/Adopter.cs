using Database.Entities;
using Database.ValueObjects;

namespace ShelterModule.Models.Adopters;

public sealed class Adopter
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required bool? IsAuthorized { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }

    public static Adopter FromRequest(AdopterRequest request)
    {
        return new Adopter
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            IsAuthorized = false,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Address = request.Address
        };
    }

    public AdopterResponse ToResponse()
    {
        return new AdopterResponse
        {
            Id = Id,
            UserName = UserName,
            IsAuthorized = IsAuthorized,
            PhoneNumber = PhoneNumber,
            Email = Email,
            Address = Address
        };
    }

    public static Adopter FromEntity(AdopterEntity entity)
    {
        return new Adopter
        {
            Id = entity.Id,
            UserName = entity.UserName,
            IsAuthorized = entity.IsAuthorized,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            Address = entity.Address
        };
    }

    public AdopterEntity ToEntity()
    {
        return new AdopterEntity
        {
            Id = Id,
            UserName = UserName,
            IsAuthorized = IsAuthorized,
            PhoneNumber = PhoneNumber,
            Email = Email,
            Address = Address
        };
    }
}
