using Database;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Shelters;
using PetShare.Services.Interfaces.Shelters;

namespace PetShare.Services.Implementations.Shelters;

public sealed class ShelterQuery : IShelterQuery
{
    private readonly PetShareDbContext _context;

    public ShelterQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Shelter>> GetAllAsync(CancellationToken token = default)
    {
        return (await _context.Shelters.ToListAsync(token)).Select(Shelter.FromEntity).ToList();
    }

    public async Task<Shelter?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Shelters.FirstOrDefaultAsync(e => e.Id == id, token);
        return entity is null ? null : Shelter.FromEntity(entity);
    }
}
