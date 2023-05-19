using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Adopters;
using PetShare.Services.Interfaces.Adopters;

namespace PetShare.Services.Implementations.Adopters;

public sealed class AdopterQuery : IAdopterQuery
{
    private readonly PetShareDbContext _context;

    public AdopterQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Adopter>> GetAllAsync(CancellationToken token = default)
    {
        return await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).
                              Select(e => Adopter.FromEntity(e)).
                              ToListAsync(token);
    }

    public async Task<Adopter?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).
                                    FirstOrDefaultAsync(e => e.Id == id, token);
        return entity is null ? null : Adopter.FromEntity(entity);
    }

    public async Task<bool?> IsVerifiedForShelterAsync(Guid id, Guid shelterId, CancellationToken token = default)
    {
        if (!await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).AnyAsync(e => e.Id == id, token))
            return null;

        return await _context.Verifications.AnyAsync(e => e.AdopterId == id && e.ShelterId == shelterId, token);
    }
}
