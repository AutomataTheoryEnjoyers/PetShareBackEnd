using PetShare.Models.Applications;

namespace PetShare.Services.Interfaces.Applications;

public interface IApplicationQuery
{
    Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken token = default);
    Task<IReadOnlyList<Application>?> GetAllForAdopterAsync(Guid adopterId, CancellationToken token = default);
    Task<IReadOnlyList<Application>?> GetAllForShelterAsync(Guid shelterId, CancellationToken token = default);
    Task<Application?> GetByIdAsync(Guid id, CancellationToken token = default);
}
