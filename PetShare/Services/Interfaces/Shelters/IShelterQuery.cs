using PetShare.Models.Shelters;

namespace PetShare.Services.Interfaces.Shelters;

public interface IShelterQuery
{
    Task<IReadOnlyList<Shelter>> GetAllAsync(CancellationToken token = default);
    Task<Shelter?> GetByIdAsync(Guid id, CancellationToken token = default);
}
