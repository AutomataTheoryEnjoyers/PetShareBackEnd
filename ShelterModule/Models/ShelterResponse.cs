using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models;

public sealed class ShelterResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required bool IsAuthorized { get; init; }
}
