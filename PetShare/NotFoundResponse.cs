using System.ComponentModel.DataAnnotations;

namespace PetShare;

public sealed class NotFoundResponse
{
    [Required]
    public required string ResourceName { get; init; }

    [Required]
    public required string Id { get; init; }
}
