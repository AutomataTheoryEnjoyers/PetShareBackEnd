using ShelterModule.Models.Shelters;

namespace ShelterModule.Services.Interfaces.Shelters;

public interface IShelterCommand
{
    public Task AddAsync(Shelter shelter, CancellationToken token = default);
    public Task<Shelter?> SetAuthorizationAsync(Guid id, bool? isAuthorized, CancellationToken token = default);
}
