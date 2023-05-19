using Database;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Shelters;
using PetShare.Services.Interfaces.Shelters;

namespace PetShare.Services.Implementations.Shelters;

public sealed class ShelterCommand : IShelterCommand
{
    private readonly PetShareDbContext _context;

    public ShelterCommand(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Shelter shelter, CancellationToken token = default)
    {
        _context.Add(shelter.ToEntity());
        await _context.SaveChangesAsync(token);
    }

    public async Task<Shelter?> SetAuthorizationAsync(Guid id, bool? isAuthorized, CancellationToken token = default)
    {
        var entity = await _context.Shelters.FirstOrDefaultAsync(e => e.Id == id, token);
        if (entity is null)
            return null;

        entity.IsAuthorized = isAuthorized;
        await _context.SaveChangesAsync(token);
        return Shelter.FromEntity(entity);
    }
}
