using System.ComponentModel.DataAnnotations;
using Database.Entities;

namespace ShelterModule.Models.Adopters;

public sealed class AdopterUpdateRequest
{
    [Required]
    public AdopterStatus Status { get; init; }
}
