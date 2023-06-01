using PetShare.Models.Pets;

namespace PetShare.Services.Interfaces.Pets;

public interface IPetQuery
{
    Task<IReadOnlyList<Pet>> GetAllForShelterAsync(Guid shelterId, CancellationToken token = default);
    Task<Pet?> GetByIdAsync(Guid id, CancellationToken token = default);
}
