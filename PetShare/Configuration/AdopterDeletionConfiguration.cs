using System.ComponentModel.DataAnnotations;

namespace PetShare.Configuration;

public sealed class AdopterDeletionConfiguration
{
    public const string SectionName = "AdopterDeletion";

    [Required]
    public int RetentionDays { get; init; }

    [Required]
    public TimeSpan DeletionPeriod { get; init; }
}
