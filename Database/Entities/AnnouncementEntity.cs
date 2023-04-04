using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public sealed class AnnouncementEntity
{
    [Key]
    public required Guid Id { get; init; }

    [Required]
    public ShelterEntity Author { get; set; } = null!;

    public required Guid AuthorId { get; set; }

    [Required]
    public PetEntity Pet { get; set; } = null!;

    public required Guid PetId { get; set; }

    public required string Title { get; set; } = null!;

    public required string Description { get; set; } = null!;

    public required DateTime CreationDate { get; init; }

    public DateTime? ClosingDate { get; set; }

    public required int Status { get; set; }

    public required DateTime LastUpdateDate { get; set; }
}
