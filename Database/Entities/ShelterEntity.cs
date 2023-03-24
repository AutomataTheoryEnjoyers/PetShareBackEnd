using System.ComponentModel.DataAnnotations;
using Database.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Entities;

public sealed class ShelterEntity
{
    [Key]
    public required Guid Id { get; init; }
    public IList<PetEnitiy> Pets { get; init; } = new List<PetEnitiy>();
    public required string UserName { get; init; }
    public required string FullShelterName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Email { get; init; }
    public required bool? IsAuthorized { get; set; }
    public required Address Address { get; init; }
}
