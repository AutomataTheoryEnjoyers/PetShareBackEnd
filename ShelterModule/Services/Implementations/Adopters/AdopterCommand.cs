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

    public async Task AddAsync(Adopter adopter, CancellationToken token = default)
    {
        _context.Adopters.Add(adopter.ToEntity());
        await _context.SaveChangesAsync(token);
    }

    public async Task<Adopter?> SetStatusAsync(Guid id, AdopterStatus status, CancellationToken token = default)
    {
        var entity = await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).
                                    FirstOrDefaultAsync(e => e.Id == id, token);
        if (entity is null)
            return null;

        entity.Status = status;
        await _context.SaveChangesAsync(token);

        return Adopter.FromEntity(entity);
    }

    public async Task<bool?> VerifyForShelterAsync(Guid id, Guid shelterId, CancellationToken token = default)
    {
        var adopterEntity = await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).
                                           FirstOrDefaultAsync(e => e.Id == id, token);
        var shelterEntity = await _context.Shelters.FirstOrDefaultAsync(e => e.Id == shelterId, token);

        if (adopterEntity is null || shelterEntity is null)
            return null;

        if (await _context.Verifications.AnyAsync(e => e.AdopterId == id && e.ShelterId == shelterId, token))
            return false;

        _context.Verifications.Add(new AdopterVerificationEntity
        {
            AdopterId = id,
            ShelterId = shelterId
        });
        await _context.SaveChangesAsync(token);
        return true;
    }
}
