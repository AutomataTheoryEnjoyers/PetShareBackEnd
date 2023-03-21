using ShelterModule.Models;

namespace ShelterModule.Services.Interfaces
{
    public interface IQuery<T>
    {
        public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken token = default);
        public Task<T?> GetByIdAsync(Guid id, CancellationToken token = default);
    }
}
