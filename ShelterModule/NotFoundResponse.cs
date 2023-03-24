using System.ComponentModel.DataAnnotations;

namespace ShelterModule;

public sealed class NotFoundResponse
{
    [Required]
    public required string ResourceName { get; init; }

    [Required]
    public required string Id { get; init; }
}
