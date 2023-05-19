using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Services.Implementations.Shelters;

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

    public async Task<IReadOnlyList<Shelter>?> GetPagedAsync(int pageNumber, int pageSize, CancellationToken token = default)
    {
        List<Shelter> allShelters = (await _context.Shelters.ToListAsync(token)).Select(Shelter.FromEntity).ToList();

        if(pageNumber*pageSize > allShelters.Count) 
            return null;

        if(pageNumber * pageSize + pageSize < allShelters.Count)
            pageSize = allShelters.Count - pageNumber * pageSize;
        
        return allShelters.GetRange(pageNumber*pageSize, pageSize);
    }


    public async Task<Shelter?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var entity = await _context.Shelters.FirstOrDefaultAsync(e => e.Id == id, token);
        return entity is null ? null : Shelter.FromEntity(entity);
    }
}
