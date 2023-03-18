using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models;

public sealed class ShelterAuthorizationRequest
{
    [Required]
    public bool IsAuthorized { get; init; }
}
