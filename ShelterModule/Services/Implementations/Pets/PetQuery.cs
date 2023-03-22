using Database;
using ShelterModule.Models.Pets;
using ShelterModule.Services.Interfaces.Pets;

namespace ShelterModule.Services.Implementations.Pets;

public class PetQuery : IPetQuery
{
    private readonly PetShareDbContext _context;

    public PetQuery(PetShareDbContext context)
    {
        _context = context;
    }

    public Task<IReadOnlyList<Pet>> GetAllAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<Pet?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
