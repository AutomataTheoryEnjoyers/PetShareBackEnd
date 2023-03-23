using ShelterModule.Models.Shelters;

namespace ShelterModule.Services.Interfaces.Shelters
{
    public interface IShelterQuery
    {
        public Task<IReadOnlyList<Shelter>> GetAllAsync(CancellationToken token = default);
        public Task<Shelter?> GetByIdAsync(Guid id, CancellationToken token = default);
    }
}
