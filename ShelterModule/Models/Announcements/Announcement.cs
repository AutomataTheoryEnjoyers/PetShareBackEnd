using ShelterModule.Models.Shelters;
using ShelterModule.Models.Pets;
using Database.Entities;

namespace ShelterModule.Models.Announcements
{
    public sealed class Announcement
    {
        public Guid Id { get; init; }
        public required Shelter Author { get; init; } = null!;
        public required Pet Pet { get; init; } = null!;
        public required string Title { get; init; } = null!;
        public required string Description { get; init; } = null!;
        public required DateTime CreationDate { get; init; }
        public DateTime? ClosingDate { get; init; } = null!;
        public required int Status { get; init; }
        public required DateTime LastUpdateDate { get; init; }

        public AnnouncementEntity ToEntity()
        {
            return new AnnouncementEntity
            {
                Id = Id,
                ShelterId = Author.Id,
                PetId = Pet.Id,  
                Title = Title,
                Description = Description,
                CreationDate = CreationDate,
                ClosingDate = ClosingDate,
                Status = Status,
                LastUpdateDate = LastUpdateDate,
            };
        }

        public static Announcement FromEntity(AnnouncementEntity entity)
        {
            return new Announcement
            {
                Id = entity.Id,
                Author = Shelter.FromEntity(entity.Author),
                Pet = Pet.FromEntity(entity.Pet),
                Title = entity.Title,
                Description = entity.Description,
                CreationDate = entity.CreationDate,
                ClosingDate = entity.ClosingDate,
                Status = entity.Status,
                LastUpdateDate = entity.LastUpdateDate,
            };
        }
        public AnnouncementResponse ToResponse()
        {
            return new AnnouncementResponse
            {
                Id = Id,
                AuthorId = Author.Id,
                PetId = Pet.Id,
                Title = Title,
                Description = Description,
                CreationDate = CreationDate,
                ClosingDate = ClosingDate,
                Status = Status,
                LastUpdateDate = LastUpdateDate,
            };
        }
        public static Announcement FromRequest(AnnouncementCreationRequest request, Shelter shelter, Pet pet)
        {
            return new Announcement
            {
                Id = Guid.NewGuid(),
                Author = shelter,
                Pet = pet,
                Title = request.Title,
                Description = request.Description,
                CreationDate = DateTime.Now,
                Status = 0,
                LastUpdateDate = DateTime.Now,
            };
        }

    }
    
}
