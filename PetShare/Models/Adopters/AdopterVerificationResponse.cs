using System.ComponentModel.DataAnnotations;

namespace PetShare.Models.Adopters;

public sealed class AdopterVerificationResponse
{
    [Required]
    public required bool IsVerified { get; init; }
}
