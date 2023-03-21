using ShelterModule.Models;

namespace ShelterModule.Services.Interfaces
{
    public interface IShelterCommand : ICommand<Shelter>
    {
        public Task<Shelter?> SetAuthorizationAsync(Guid id, bool isAuthorized);
    }
}
