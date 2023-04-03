using System.ComponentModel.DataAnnotations;
using Database.ValueObjects;

namespace ShelterModule.Models.Adopters;

public sealed class AdopterRequest
{
    [Required]
    public string UserName { get; init; } = null!;

    [Required]
    [Phone]
    public string PhoneNumber { get; init; } = null!;

    [Required]
    [EmailAddress]
    public required string Email { get; init; } = null!;

    [Required]
    public required Address Address { get; init; } = null!;
}
