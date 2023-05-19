using System.ComponentModel.DataAnnotations;
using Database.Entities;

namespace ShelterModule.Models.Pets;

public sealed class PetCreationRequest : IValidatableObject
{
    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Species { get; init; }

    [Required]
    public required string Breed { get; init; }

    [Required]
    public required DateTime Birthday { get; init; }

    [Required]
    public required string Description { get; init; }

    [Required]
    public required string PhotoUrl { get; init; }

    [Required]
    public required string Sex { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.TryParse<PetSex>(Sex, true, out _))
            yield return new ValidationResult("Sex has an invalid value", new[] { nameof(Sex) });
    }
}
