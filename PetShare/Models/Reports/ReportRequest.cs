using System.ComponentModel.DataAnnotations;
using Database.Entities;

namespace PetShare.Models.Reports;

public sealed class ReportRequest : IValidatableObject
{
    [Required]
    public required Guid TargetId { get; init; }

    [Required]
    public required string ReportType { get; init; }

    [Required]
    public required string Message { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.TryParse<ReportedEntityType>(ReportType, true, out _))
            yield return new ValidationResult($"{ReportType} is not a valid report target type",
                                              new[] { nameof(ReportType) });
    }
}
