using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Adopters;

public sealed class AdopterVerificationResponse
{
    [Required]
    public required bool IsVerified { get; init; }
}
