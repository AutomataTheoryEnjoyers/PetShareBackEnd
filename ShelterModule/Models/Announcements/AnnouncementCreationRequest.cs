using ShelterModule.Models.Pets;
using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Announcements
{
    public sealed class AnnouncementCreationRequest
    {
        [Required]
        public string Title { get; init; } = null!;
        [Required]
        public string Description { get; init; } = null!;
        [Required]
        public Guid ShelterId { get; init; }
        public Guid? PetId { get; init; } = null!;

        public PetUpsertRequest? PetRequest { get; init; } = null!;
    }
}
