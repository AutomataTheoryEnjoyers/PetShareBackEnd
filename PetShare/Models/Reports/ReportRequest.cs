using System.ComponentModel.DataAnnotations;
using Database.Entities;

namespace PetShare.Models.Reports;

public sealed class ReportRequest : IValidatableObject
{
    [Required]
    public required Guid TargetId { get; init; }

    [Required]
    public required string Type { get; init; }

    [Required]
    public required string Message { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.TryParse<ReportedEntityType>(Type, true, out _))
            yield return new ValidationResult($"{Type} is not a valid report target type", new[] { nameof(Type) });
    }
}
