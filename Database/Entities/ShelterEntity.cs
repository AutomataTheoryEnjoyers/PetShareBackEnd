using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public sealed class ShelterEntity
{
    [Key]
    public required Guid Id { get; init; }

    public required string UserName { get; init; }
    public required string FullShelterName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Email { get; init; }
    public required bool? IsAuthorized { get; set; }
}
