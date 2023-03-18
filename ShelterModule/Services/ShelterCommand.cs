using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models;

namespace ShelterModule.Services;

public sealed class ShelterCommand
{
    private readonly PetShareDbContext _context;

    public ShelterCommand(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Shelter shelter)
    {
        _context.Add(shelter.ToEntity());
        await _context.SaveChangesAsync();
    }

    public async Task<Shelter?> SetAuthorizationAsync(Guid id, bool isAuthorized)
    {
        var entity = await _context.Shelters.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
            return null;

        entity.IsAuthorized = isAuthorized;
        await _context.SaveChangesAsync();
        return Shelter.FromEntity(entity);
    }
}
