using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models;

public sealed class ShelterCreationRequest
{
    [Required]
    [MaxLength(20)]
    public string UserName { get; init; } = null!;
    
    [Required]
    public string FullShelterName { get; init; } = null!;
    [Required]
    public string PhoneNumber { get; init; } = null!;
    [Required]
    public string Email { get; init; } = null!;
}
