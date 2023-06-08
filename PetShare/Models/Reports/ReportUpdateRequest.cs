using System.ComponentModel.DataAnnotations;
using Database.Entities;

namespace PetShare.Models.Reports;

public sealed class ReportUpdateRequest : IValidatableObject
{
    [Required]
    public required string State { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.TryParse<ReportState>(State, true, out _))
            yield return new ValidationResult($"'{State}' is not a valid report state", new[] { nameof(State) });
    }
}
