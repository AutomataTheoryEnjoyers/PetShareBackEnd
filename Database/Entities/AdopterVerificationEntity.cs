using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

[PrimaryKey(nameof(AdopterId), nameof(ShelterId))]
public sealed class AdopterVerificationEntity
{
    public required Guid AdopterId { get; init; }

    [Required]
    public AdopterEntity Adopter { get; init; } = null!;

    public required Guid ShelterId { get; init; }

    [Required]
    public ShelterEntity Shelter { get; init; } = null!;
}
