using Database;
using ShelterModule.Models.Pets;
using ShelterModule.Services.Interfaces.Pets;

namespace ShelterModule.Services.Implementations.Pets;

public class PetCommand : IPetCommand
{
    private readonly PetShareDbContext _dbContext;

    public PetCommand(PetShareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Pet typeObject)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(Pet typeObject)
    {
        throw new NotImplementedException();
    }
}
