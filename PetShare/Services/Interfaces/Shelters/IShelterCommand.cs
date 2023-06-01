using PetShare.Models.Shelters;

namespace PetShare.Services.Interfaces.Shelters;

public interface IShelterCommand
{
    Task AddAsync(Shelter shelter, CancellationToken token = default);
    Task<Shelter?> SetAuthorizationAsync(Guid id, bool? isAuthorized, CancellationToken token = default);
}
