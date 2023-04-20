using Database;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Pet> AddAsync(Pet pet, CancellationToken token = default)
    {
        _dbContext.Pets.Add(pet.ToEntity());
        await _dbContext.SaveChangesAsync(token);
        return pet;
    }

    public async Task<Pet?> UpdateAsync(Guid id, PetUpdateRequest request, CancellationToken token = default)
    {
        var entity = await _dbContext.Pets.FirstOrDefaultAsync(e => e.Id == id, token);
        if (entity is null)
            return null;

        entity.Name = request.Name ?? entity.Name;
        entity.Breed = request.Breed ?? entity.Breed;
        entity.Species = request.Species ?? entity.Species;
        entity.Birthday = request.Birthday ?? entity.Birthday;
        entity.Description = request.Description ?? entity.Description;

        await _dbContext.SaveChangesAsync(token);

        return Pet.FromEntity(entity);
    }

    public async Task<Pet?> SetPhotoAsync(Guid id, IFormFile photo, CancellationToken token = default)
    {
        var entity = await _dbContext.Pets.FirstOrDefaultAsync(e => e.Id == id, token);
        if (entity is null)
            return null;

        entity.Photo = "some-url.jpg"; // TODO: Implement
        await _dbContext.SaveChangesAsync(token);

        return Pet.FromEntity(entity);
    }
}
