using ShelterModule.Models.Adopters;
using ShelterModule.Models.Shelters;

namespace ShelterModule.Services.Interfaces.Adopters;

public interface IAdopterQuery
{
    Task<IReadOnlyList<Adopter>> GetAllAsync(CancellationToken token = default);
    Task<Adopter?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<bool?> IsVerifiedForShelterAsync(Guid id, Guid shelterId, CancellationToken token = default);
    public Task<IReadOnlyList<Adopter>?> GetPagedAsync(int pageNumber, int pageSize, CancellationToken token = default);
}
