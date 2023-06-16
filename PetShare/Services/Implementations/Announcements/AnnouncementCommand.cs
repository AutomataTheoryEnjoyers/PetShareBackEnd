using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Announcements;
using PetShare.Results;
using PetShare.Services.Interfaces.Announcements;

namespace PetShare.Services.Implementations.Announcements;

public class AnnouncementCommand : IAnnouncementCommand
{
    private readonly PetShareDbContext _dbContext;

    public AnnouncementCommand(PetShareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> AddAsync(Announcement announcement, CancellationToken token = default)
    {
        if (!await _dbContext.Shelters.AnyAsync(shelter => shelter.Id == announcement.AuthorId, token))
            return new InvalidOperation($"There is no shelter with ID {announcement.AuthorId}");

        if (!await _dbContext.Pets.AnyAsync(pet => pet.Id == announcement.Pet.Id, token))
            return new InvalidOperation($"There is no pet with ID {announcement.Pet.Id}");

        var entityAnnouncement = announcement.ToEntity();
        _dbContext.Announcements.Add(entityAnnouncement);
        await _dbContext.SaveChangesAsync(token);
        return Result.Ok;
    }

    public async Task<Announcement?> UpdateAsync(Guid id, AnnouncementPutRequest request,
        CancellationToken token = default)
    {
        var entityToUpdate = await _dbContext.Announcements.Where(e => e.Status != AnnouncementStatus.Deleted).
                                              Include(e => e.Pet.Shelter).
                                              FirstOrDefaultAsync(e => e.Id == id, token);
        if (entityToUpdate is null)
            return null;

        entityToUpdate.Title = request.Title ?? entityToUpdate.Title;
        entityToUpdate.Description = request.Description ?? entityToUpdate.Description;
        entityToUpdate.Status = request.Status is null
            ? entityToUpdate.Status
            : Enum.Parse<AnnouncementStatus>(request.Status);

        if (entityToUpdate.Status == AnnouncementStatus.Closed)
            entityToUpdate.ClosingDate = DateTime.Now;
        entityToUpdate.LastUpdateDate = DateTime.Now;

        await _dbContext.SaveChangesAsync(token);
        return Announcement.FromEntity(entityToUpdate);
    }

    public Task<Result> LikeAsync(Guid id, Guid adopterId, bool isLiked, CancellationToken token = default)
    {
        return isLiked ? AddLikeAsync(id, adopterId, token) : RemoveLikeAsync(id, adopterId, token);
    }

    private async Task<Result> AddLikeAsync(Guid id, Guid adopterId, CancellationToken token = default)
    {
        if (!await _dbContext.Announcements.AnyAsync(announcement => announcement.Id == id, token))
            return new AnnouncementNotFound(id);

        if (!await _dbContext.Adopters.AnyAsync(adopter => adopter.Id == adopterId, token))
            return new AdopterNotFound(adopterId);

        if (await _dbContext.Likes.AnyAsync(like => like.AdopterId == adopterId && like.AnnouncementId == id, token))
            return new InvalidOperation("Announcement is already liked");

        _dbContext.Likes.Add(new LikedAnnouncementEntity
        {
            AdopterId = adopterId,
            AnnouncementId = id
        });
        await _dbContext.SaveChangesAsync(token);

        return Result.Ok;
    }

    private async Task<Result> RemoveLikeAsync(Guid id, Guid adopterId, CancellationToken token = default)
    {
        if (!await _dbContext.Announcements.AnyAsync(announcement => announcement.Id == id, token))
            return new AnnouncementNotFound(id);

        if (!await _dbContext.Adopters.AnyAsync(adopter => adopter.Id == adopterId, token))
            return new AdopterNotFound(adopterId);

        var entity =
            await _dbContext.Likes.FirstOrDefaultAsync(like => like.AdopterId == adopterId && like.AnnouncementId == id,
                                                       token);
        if (entity is null)
            return new InvalidOperation("Announcement is not liked");

        _dbContext.Likes.Remove(entity);
        await _dbContext.SaveChangesAsync(token);

        return Result.Ok;
    }
}
