using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using PetShare.Models.Pets;
using PetShare.Services.Interfaces.Pets;

namespace PetShare.Services.Implementations.Pets;

public sealed class PetQuery : IPetQuery
{
    private readonly PetShareDbContext _context;

    public PetQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pet>> GetAllForShelterAsync(Guid shelterId, CancellationToken token = default)
    {
        return (await _context.Pets.Include(x => x.Shelter).
                               Where(e => e.Status != PetStatus.Deleted).
                               Where(pet => pet.ShelterId == shelterId).
                               ToListAsync(token)).Select(Pet.FromEntity).
                                                   ToList();
    }

    public async Task<Pet?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Pets.Where(e => e.Status != PetStatus.Deleted).
                                    Include(x => x.Shelter).
                                    FirstOrDefaultAsync(e => e.Id == id, token);
        return entity is null ? null : Pet.FromEntity(entity);
    }
}
