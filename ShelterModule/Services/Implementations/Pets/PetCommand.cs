using Database;
using Database.Entities;
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

        public async Task<Pet?> UpdateAsync(Guid id, PetUpsertRequest request)
        {
            var entityToUpdate = await _dbContext.Pets.FirstOrDefaultAsync(e => e.Id == id);
            if (entityToUpdate is null)
                return null;

            entityToUpdate.Name = request.Name;
            entityToUpdate.Breed = request.Breed;
            entityToUpdate.Species = request.Species;
            entityToUpdate.Birthday = request.Birthday;
            entityToUpdate.Description = request.Description;
            entityToUpdate.Photo = request.Photo;
            entityToUpdate.ShelterId = request.ShelterId;

            await _dbContext.SaveChangesAsync();
            
            var updatedEntity = await _dbContext.Pets.Include(x => x.Shelter).FirstOrDefaultAsync(e => e.Id == id);

            return Pet.FromEntity(updatedEntity!);
        }
    }
}
