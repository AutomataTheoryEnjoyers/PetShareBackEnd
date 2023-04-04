using System.ComponentModel.DataAnnotations;
using ShelterModule.Models.Pets;

namespace ShelterModule.Models.Announcements;

public sealed class AnnouncementCreationRequest : IValidatableObject
{
    [Required]
    public string Title { get; init; } = null!;

    [Required]
    public string Description { get; init; } = null!;

    [Required]
    public Guid ShelterId { get; init; }

    public Guid? PetId { get; init; }

    public PetUpsertRequest? PetRequest { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();
        if (PetId is null)
        {
            if (PetRequest is null)
            {
                results.Add(new ValidationResult("PetId and PetRequest can't both be null at the same time"));
            }
            else
            {
                Validator.TryValidateObject(PetRequest, new ValidationContext(PetRequest), results, true);
            }       
        }
        return results;
    }
}
