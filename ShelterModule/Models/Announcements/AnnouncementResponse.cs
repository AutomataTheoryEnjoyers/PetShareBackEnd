using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Announcements
{
    public sealed class AnnouncementResponse
    {
        
        public required Guid Id { get; init; }
        
        public required Guid AuthorId { get; init; } 
        
        public required Guid PetId { get; init; }
        
        public required string Title { get; init; } = null!;
        
        public required string Description { get; init; } = null!;
        
        public required DateTime CreationDate { get; init; }

        public DateTime? ClosingDate { get; init; } = null!;
        
        public required int Status { get; init; }
        
        public required DateTime LastUpdateDate { get; init; }
    }
}
