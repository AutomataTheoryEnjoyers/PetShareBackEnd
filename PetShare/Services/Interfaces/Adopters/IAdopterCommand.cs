using Database.Entities;
using PetShare.Models.Adopters;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Adopters;

public interface IAdopterCommand
{
    Task AddAsync(Adopter adopter, CancellationToken token = default);
    Task<Adopter?> SetStatusAsync(Guid id, AdopterStatus status, CancellationToken token = default);
    Task<Result> VerifyForShelterAsync(Guid id, Guid shelterId, CancellationToken token = default);
    Task RemoveDeletedAsync(DateTime limit, CancellationToken token = default);
}
