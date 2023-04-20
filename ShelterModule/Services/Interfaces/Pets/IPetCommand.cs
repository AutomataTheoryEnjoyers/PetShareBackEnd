using ShelterModule.Models.Pets;

namespace ShelterModule.Services.Interfaces.Pets;

public interface IPetCommand
{
    public Task<Pet> AddAsync(Pet pet, CancellationToken token = default);
    public Task<Pet?> UpdateAsync(Guid id, PetUpdateRequest request, CancellationToken token = default);
    public Task<Pet?> SetPhotoAsync(Guid id, IFormFile photo, CancellationToken token = default);
}
