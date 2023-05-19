using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Announcements;
using PetShare.Services.Interfaces.Announcements;

namespace PetShare.Services.Implementations.Announcements;

public class AnnouncementCommand : IAnnouncementCommand
{
    private readonly PetShareDbContext _dbContext;

    public AnnouncementCommand(PetShareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Announcement> AddAsync(Announcement announcement, CancellationToken token = default)
    {
        var entityAnnouncement = announcement.ToEntity();
        _dbContext.Announcements.Add(entityAnnouncement);
        await _dbContext.SaveChangesAsync(token);
        return announcement;
    }

    public async Task<Announcement?> UpdateAsync(Guid id, AnnouncementPutRequest request,
        CancellationToken token = default)
    {
        var entityToUpdate = await _dbContext.Announcements.FirstOrDefaultAsync(e => e.Id == id, token);
        if (entityToUpdate is null)
            return null;

        entityToUpdate.Title = request.Title ?? entityToUpdate.Title;
        entityToUpdate.Description = request.Description ?? entityToUpdate.Description;
        entityToUpdate.Status = request.Status is null ? entityToUpdate.Status : Enum.Parse<AnnouncementStatus>(request.Status);

        if (entityToUpdate.Status == AnnouncementStatus.Closed)
            entityToUpdate.ClosingDate = DateTime.Now;
        entityToUpdate.LastUpdateDate = DateTime.Now;

        await _dbContext.SaveChangesAsync(token);
        return Announcement.FromEntity(entityToUpdate);
    }
}
