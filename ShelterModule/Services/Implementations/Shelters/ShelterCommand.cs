using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Services.Implementations.Shelters;

public sealed class ShelterCommand : IShelterCommand
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

    public async Task RemoveAsync(Shelter shelter)
    {
        var entityToRemove = await _context.Shelters.FirstOrDefaultAsync(e => e.Id == shelter.Id);
        if( entityToRemove != null)
        {
            _context.Remove(entityToRemove);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Shelter?> SetAuthorizationAsync(Guid id, bool? isAuthorized)
    {
        var entity = await _context.Shelters.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
            return null;

        entity.IsAuthorized = isAuthorized;
        await _context.SaveChangesAsync();
        return Shelter.FromEntity(entity);
    }
}
