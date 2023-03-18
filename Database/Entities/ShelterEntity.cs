using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public sealed class ShelterEntity
{
    [Key]
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required bool IsAuthorized { get; set; }
}
