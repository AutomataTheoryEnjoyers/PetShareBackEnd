using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Adopters;
using ShelterModule.Services.Interfaces.Adopters;

namespace ShelterModule.Services.Implementations.Adopters;

public sealed class AdopterQuery : IAdopterQuery
{
    private readonly PetShareDbContext _context;

    public AdopterQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Adopter>> GetAllAsync()
    {
        return await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).
                              Select(e => Adopter.FromEntity(e)).
                              ToListAsync();
    }

    public async Task<Adopter?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).
                                    FirstOrDefaultAsync(e => e.Id == id);
        return entity is null ? null : Adopter.FromEntity(entity);
    }

    public async Task<bool?> IsVerifiedForShelterAsync(Guid id, Guid shelterId)
    {
        if (!await _context.Adopters.Where(e => e.Status != AdopterStatus.Deleted).AnyAsync(e => e.Id == id))
            return null;

        return await _context.Verifications.AnyAsync(e => e.AdopterId == id && e.ShelterId == shelterId);
    }
}
