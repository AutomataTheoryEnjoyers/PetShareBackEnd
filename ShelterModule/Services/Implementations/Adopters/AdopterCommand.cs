using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Adopters;
using ShelterModule.Services.Interfaces.Adopters;

namespace ShelterModule.Services.Implementations.Adopters;

public sealed class AdopterCommand : IAdopterCommand
{
    private readonly PetShareDbContext _context;

    public AdopterCommand(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Adopter adopter)
    {
        _context.Adopters.Add(adopter.ToEntity());
        await _context.SaveChangesAsync();
    }

    public async Task<Adopter?> SetStatusAsync(Guid id, AdopterStatus status)
    {
        var entity = await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).
                                    FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
            return null;

        entity.Status = status;
        await _context.SaveChangesAsync();

        return Adopter.FromEntity(entity);
    }

    public async Task<bool?> VerifyForShelterAsync(Guid id, Guid shelterId)
    {
        var adopterEntity = await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).
                                           FirstOrDefaultAsync(e => e.Id == id);
        var shelterEntity = await _context.Shelters.FirstOrDefaultAsync(e => e.Id == shelterId);

        if (adopterEntity is null || shelterEntity is null)
            return null;

        if (await _context.Verifications.AnyAsync(e => e.AdopterId == id && e.ShelterId == shelterId))
            return false;

        _context.Verifications.Add(new AdopterVerificationEntity
        {
            AdopterId = id,
            ShelterId = shelterId
        });
        await _context.SaveChangesAsync();
        return true;
    }
}
