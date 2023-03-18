using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models;

public sealed class ShelterCreationRequest
{
    [Required]
    [MaxLength(20)]
    public string Name { get; init; } = null!;
}
