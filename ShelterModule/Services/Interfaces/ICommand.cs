using ShelterModule.Models;

namespace ShelterModule.Services.Interfaces
{
    public interface ICommand<T>
    {
        public Task AddAsync(T typeObject);
        public Task RemoveAsync(T typeObject);
        //public Task<T> UpdateByIdAsync(Guid id, T typeObject);
    }
}
