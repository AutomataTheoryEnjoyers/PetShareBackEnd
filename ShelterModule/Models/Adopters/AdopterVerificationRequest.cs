using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Adopters;

public sealed class AdopterVerificationRequest
{
    [Required]
    public bool IsVerified { get; init; }
}
