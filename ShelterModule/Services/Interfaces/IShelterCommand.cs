using ShelterModule.Models.Shelter;

namespace ShelterModule.Services.Interfaces
{
    public interface IShelterCommand : ICommand<Shelter>
    {
        public Task<Shelter?> SetAuthorizationAsync(Guid id, bool? isAuthorized);
    }
}
