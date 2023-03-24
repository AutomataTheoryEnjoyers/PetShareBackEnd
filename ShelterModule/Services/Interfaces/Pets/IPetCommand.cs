using ShelterModule.Models.Pets;

namespace ShelterModule.Services.Interfaces.Pets;

public interface IPetCommand
{
    public Task<Pet> AddAsync(Pet pet, CancellationToken token = default);
    public Task<Pet?> UpdateAsync(Guid id, PetUpsertRequest request, CancellationToken token = default);
}
