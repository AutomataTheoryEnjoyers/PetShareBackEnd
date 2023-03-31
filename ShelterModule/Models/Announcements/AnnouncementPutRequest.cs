using ShelterModule.Models.Pets;
using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Announcements
{
    public sealed class AnnouncementPutRequest
    {
        
        public string? Title { get; init; } = null!;
      
        public string? Description { get; init; } = null!;

        public Guid? PetId { get; init; } = null!;
        
        public Guid? ShelterId { get; init; }

        public int? Status { get; init; } = null!;

        
    }
}
