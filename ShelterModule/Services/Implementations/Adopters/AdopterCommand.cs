using Database;
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

    public async Task<Adopter?> VerifyAsync(Guid id, bool verificationStatus)
    {
        var entity = await _context.Adopters.FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null)
            return null;

        entity.IsAuthorized = verificationStatus;
        await _context.SaveChangesAsync();
        return Adopter.FromEntity(entity);
    }
}
