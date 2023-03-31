using System.ComponentModel.DataAnnotations;

namespace ShelterModule;

public sealed class UnauthorizedResponse
{
    [Required]
    public required string ResourceName { get; init; }

    [Required]
    public required string Id { get; init; }
}
