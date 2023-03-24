using Database.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Shelters;

public sealed class ShelterResponse
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string FullShelterName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required bool? IsAuthorized { get; init; }
    public required Address Address { get; init; }
}
