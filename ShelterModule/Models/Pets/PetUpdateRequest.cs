using System.ComponentModel.DataAnnotations;
using Database.Entities;

namespace ShelterModule.Models.Pets;

public sealed class PetUpdateRequest : IValidatableObject
{
    public string? Name { get; init; }

    public string? Species { get; init; }

    public string? Breed { get; init; }

    public DateTime? Birthday { get; init; }

    public string? Description { get; init; }

    public string? Status { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status is not null && !Enum.TryParse<PetStatus>(Status, true, out _))
            yield return new ValidationResult("Status has an invalid value", new[] { nameof(Status) });
    }
}
