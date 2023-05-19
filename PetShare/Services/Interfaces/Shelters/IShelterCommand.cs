using PetShare.Models.Shelters;

namespace PetShare.Services.Interfaces.Shelters;

public interface IShelterCommand
{
    public Task AddAsync(Shelter shelter, CancellationToken token = default);
    public Task<Shelter?> SetAuthorizationAsync(Guid id, bool? isAuthorized, CancellationToken token = default);
}
