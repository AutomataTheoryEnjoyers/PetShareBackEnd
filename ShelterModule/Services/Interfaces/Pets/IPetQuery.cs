using ShelterModule.Models.Pets;

namespace ShelterModule.Services.Interfaces.Pets;

public interface IPetQuery
{
    public Task<IReadOnlyList<Pet>> GetAllAsync(CancellationToken token = default);
    public Task<Pet?> GetByIdAsync(Guid id, CancellationToken token = default);
}
