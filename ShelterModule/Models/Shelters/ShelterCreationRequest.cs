using System.ComponentModel.DataAnnotations;
using Database.ValueObjects;

namespace ShelterModule.Models.Shelters;

public sealed class ShelterCreationRequest
{
    [Required]
    public string UserName { get; init; } = null!;

    [Required]
    public string FullShelterName { get; init; } = null!;

    [Required]
    [Phone]
    public string PhoneNumber { get; init; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;

    [Required]
    public Address Address { get; init; } = null!;
}
