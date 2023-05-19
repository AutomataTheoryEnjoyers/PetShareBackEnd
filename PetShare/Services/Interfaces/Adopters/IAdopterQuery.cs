using PetShare.Models.Adopters;

namespace PetShare.Services.Interfaces.Adopters;

public interface IAdopterQuery
{
    Task<IReadOnlyList<Adopter>> GetAllAsync(CancellationToken token = default);
    Task<Adopter?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<bool?> IsVerifiedForShelterAsync(Guid id, Guid shelterId, CancellationToken token = default);
}
