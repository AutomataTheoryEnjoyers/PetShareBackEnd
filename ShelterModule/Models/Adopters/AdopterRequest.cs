using System.ComponentModel.DataAnnotations;
using Database.ValueObjects;

namespace ShelterModule.Models.Adopters;

public sealed class AdopterRequest
{
    [Required]
    public required string UserName { get; init; }

    [Required]
    [Phone]
    public required string PhoneNumber { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required Address Address { get; init; }
}
