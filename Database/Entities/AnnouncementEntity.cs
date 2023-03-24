using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public sealed class AnnouncementEntity
    {
        [Key]
        
        public required Guid Id { get; init; }

        public required Guid ShelterId { get; set; }
        public required ShelterEntity Author { get; set; } = null!;
        
        public required Guid PetId { get; set; }
        public required PetEnitiy Pet { get; set; } = null!;
        
        public required string Title { get; set; } = null!;
        
        public required string Description { get; set; } = null!;
        
        public required DateTime CreationDate { get; init; }

        public DateTime? ClosingDate { get; set; } = null!;
        
        public required int Status { get; set; }
        
        public required DateTime LastUpdateDate { get; set; }

    }
    
}
