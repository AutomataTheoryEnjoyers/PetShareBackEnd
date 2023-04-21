using Database.Entities;
using Database.ValueObjects;

namespace ShelterModule.Models.Adopters;

public sealed class Adopter
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }
    public required AdopterStatus Status { get; init; }

    public static Adopter FromRequest(AdopterRequest request)
    {
        return new Adopter
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Address = request.Address,
            Status = AdopterStatus.Active
        };
    }

    public AdopterResponse ToResponse()
    {
        return new AdopterResponse
        {
            Id = Id,
            UserName = UserName,
            PhoneNumber = PhoneNumber,
            Email = Email,
            Address = Address,
            Status = Status
        };
    }

    public static Adopter FromEntity(AdopterEntity entity)
    {
        return new Adopter
        {
            Id = entity.Id,
            UserName = entity.UserName,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            Address = entity.Address,
            Status = entity.Status
        };
    }

    public AdopterEntity ToEntity()
    {
        return new AdopterEntity
        {
            Id = Id,
            UserName = UserName,
            PhoneNumber = PhoneNumber,
            Email = Email,
            Address = Address,
            Status = Status
        };
    }
}
