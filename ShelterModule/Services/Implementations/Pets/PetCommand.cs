using Database;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Pets;

namespace ShelterModule.Services.Implementations.Pets;

public class PetCommand : IPetCommand
{
    private readonly PetShareDbContext _dbContext;

    public PetCommand(PetShareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Pet> AddAsync(Pet pet, CancellationToken token = default)
    {
        _dbContext.Pets.Add(pet.ToEntity());
        await _dbContext.SaveChangesAsync(token);
        return pet;
    }

    public async Task<Pet?> UpdateAsync(Guid id, PetUpsertRequest request, CancellationToken token = default)
    {
        var entityToUpdate = await _dbContext.Pets.FirstOrDefaultAsync(e => e.Id == id, token);
        if (entityToUpdate is null)
            return null;

        entityToUpdate.Name = request.Name;
        entityToUpdate.Breed = request.Breed;
        entityToUpdate.Species = request.Species;
        entityToUpdate.Birthday = request.Birthday;
        entityToUpdate.Description = request.Description;
        entityToUpdate.Photo = request.Photo;
        entityToUpdate.ShelterId = request.ShelterId;

        await _dbContext.SaveChangesAsync(token);

        return Pet.FromEntity(entityToUpdate);
    }
}
