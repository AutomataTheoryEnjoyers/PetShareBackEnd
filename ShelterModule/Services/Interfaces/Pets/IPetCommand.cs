using ShelterModule.Models.Pets;

namespace ShelterModule.Services.Interfaces.Pets
{
    public interface IPetCommand
    {
        public Task<Pet> AddAsync(Pet pet);
        public Task RemoveAsync(Pet pet);
        public Task<Pet?> UpdateAsync(Pet pet);
    }
}
