using System.ComponentModel.DataAnnotations;
using Database.ValueObjects;

namespace ShelterModule.Models.Adopters;

public sealed class AdopterResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required string UserName { get; init; }

    [Required]
    public required bool? IsAuthorized { get; init; }

    [Required]
    public required string PhoneNumber { get; init; }

    [Required]
    public required string Email { get; init; }

    [Required]
    public required Address Address { get; init; }
}
