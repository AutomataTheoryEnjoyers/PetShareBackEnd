using ShelterModule.Models.Shelters;

namespace ShelterModule.Services.Interfaces.Shelters;

public interface IShelterCommand : ICommand<Shelter>
{
    public Task<Shelter?> SetAuthorizationAsync(Guid id, bool? isAuthorized);
}
