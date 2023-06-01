using PetShare.Models.Pets;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Pets;

public interface IPetCommand
{
    Task AddAsync(Pet pet, CancellationToken token = default);
    Task<Pet?> UpdateAsync(Guid id, PetUpdateRequest request, CancellationToken token = default);
    Task<Result<Pet>> SetPhotoAsync(Guid id, IFormFile photo, CancellationToken token = default);
}
