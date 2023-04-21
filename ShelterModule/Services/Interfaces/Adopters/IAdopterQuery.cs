using ShelterModule.Models.Adopters;

namespace ShelterModule.Services.Interfaces.Adopters;

public interface IAdopterQuery
{
    Task<IReadOnlyList<Adopter>> GetAllAsync();
    Task<Adopter?> GetByIdAsync(Guid id);
    Task<bool?> IsVerifiedForShelterAsync(Guid id, Guid shelterId);
}
