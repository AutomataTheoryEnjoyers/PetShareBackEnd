using PetShare.Models.Pets;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Pets;

public interface IPetCommand
{
    public Task AddAsync(Pet pet, CancellationToken token = default);
    public Task<Pet?> UpdateAsync(Guid id, PetUpdateRequest request, CancellationToken token = default);
    public Task<Result<Pet>> SetPhotoAsync(Guid id, IFormFile photo, CancellationToken token = default);
}
