using ShelterModule.Models.Pets;

namespace ShelterModule.Services.Interfaces.Pets
{
    public interface IPetCommand
    {
        public Task AddAsync(Pet pet);
        public Task RemoveAsync(Pet pet);
    }
}
