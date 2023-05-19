using Database.Entities;
using PetShare.Models.Adopters;

namespace PetShare.Services.Interfaces.Adopters;

public interface IAdopterCommand
{
    Task AddAsync(Adopter adopter, CancellationToken token = default);
    Task<Adopter?> SetStatusAsync(Guid id, AdopterStatus status, CancellationToken token = default);
    Task<bool?> VerifyForShelterAsync(Guid id, Guid shelterId, CancellationToken token = default);
}
