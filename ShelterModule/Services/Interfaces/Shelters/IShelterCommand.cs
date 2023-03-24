using ShelterModule.Models.Shelters;

namespace ShelterModule.Services.Interfaces.Shelters
{
    public interface IShelterCommand
    {
        public Task AddAsync(Shelter shelter);
        public Task RemoveAsync(Shelter shelter);
        public Task<Shelter?> SetAuthorizationAsync(Guid id, bool? isAuthorized);
    }
}
