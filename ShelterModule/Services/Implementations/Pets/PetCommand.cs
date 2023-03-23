using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Pets;

namespace ShelterModule.Services.Implementations.Pets
{
    public class PetCommand : IPetCommand
    {
        private readonly PetShareDbContext _dbContext;

        public PetCommand(PetShareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Pet> AddAsync(Pet pet)
        {
            _dbContext.Add(pet.ToEntity());
            await _dbContext.SaveChangesAsync();
            return pet;
        }

        public async Task RemoveAsync(Pet pet)
        {
            var entityToRemove = await _dbContext.Pets.FirstOrDefaultAsync(e => e.Id == pet.Id);
            if (entityToRemove != null)
            {
                _dbContext.Remove(entityToRemove);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<Pet?> UpdateAsync(Pet pet)
        {
            var entityToUpdate = await _dbContext.Pets.FirstOrDefaultAsync(e => e.Id == pet.Id);
            if (entityToUpdate is null)
                return null;
            
            entityToUpdate = pet.ToEntity();
            await _dbContext.SaveChangesAsync();
            return Pet.FromEntity(entityToUpdate);
        }
    }
}
