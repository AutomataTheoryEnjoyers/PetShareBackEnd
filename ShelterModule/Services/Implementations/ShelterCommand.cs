using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models;
using ShelterModule.Services.Interfaces;

namespace ShelterModule.Services.Implementations;

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

    public async Task RemoveAsync(Shelter typeObject)
    {
        _context.Remove(typeObject);
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

    public Task UpdateByIdAsync(Guid id, Shelter typeObject)
    {
        // TO DO: Get then update all changable fields than update and save changes 
        throw new NotImplementedException();
    }
}
