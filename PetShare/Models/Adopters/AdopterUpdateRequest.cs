using System.ComponentModel.DataAnnotations;
using Database.Entities;

namespace PetShare.Models.Adopters;

public sealed class AdopterUpdateRequest
{
    [Required]
    public AdopterStatus Status { get; init; }
}
