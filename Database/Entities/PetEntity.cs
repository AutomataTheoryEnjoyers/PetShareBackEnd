using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities;

public sealed class PetEntity
{
    [Key]
    public required Guid Id { get; init; }

    [Required]
    //[ForeignKey("ShelterId")]
    public ShelterEntity Shelter { get; set; } = null!;
    public List<AnnouncementEntity> Announcements { get; set; } = null!;
    public required Guid ShelterId { get; set; }
    public string Name { get; set; } = null!;
    public string Species { get; set; } = null!;
    public string Breed { get; set; } = null!;
    public DateTime Birthday { get; set; }
    public string Description { get; set; } = null!;
    public string Photo { get; set; } = null!;
}
