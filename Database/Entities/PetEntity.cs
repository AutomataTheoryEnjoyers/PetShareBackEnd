using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public sealed class PetEntity
{
    [Key]
    public required Guid Id { get; init; }

    [Required]
    public ShelterEntity Shelter { get; set; } = null!;

    public List<AnnouncementEntity> Announcements { get; set; } = new();
    public required Guid ShelterId { get; set; }
    public required string Name { get; set; }
    public required string Species { get; set; }
    public required string Breed { get; set; }
    public required DateTime Birthday { get; set; }
    public required string Description { get; set; }
    public string? Photo { get; set; }
    public required PetSex Sex { get; set; }
    public required PetStatus Status { get; set; }
}

public enum PetStatus
{
    Active,
    Deleted
}

public enum PetSex
{
    Unknown,
    Male,
    Female,
    DoesNotApply
}
