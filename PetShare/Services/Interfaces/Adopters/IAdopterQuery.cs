using PetShare.Models.Adopters;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Adopters;

public interface IAdopterQuery
{
    Task<IReadOnlyList<Adopter>> GetAllAsync(CancellationToken token = default);
    Task<Adopter?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<Result<bool>> IsVerifiedForShelterAsync(Guid id, Guid shelterId, CancellationToken token = default);
}
