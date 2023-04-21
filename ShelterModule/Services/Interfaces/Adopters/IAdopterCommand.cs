using Database.Entities;
using ShelterModule.Models.Adopters;

namespace ShelterModule.Services.Interfaces.Adopters;

public interface IAdopterCommand
{
    Task AddAsync(Adopter adopter, CancellationToken token = default);
    Task<Adopter?> SetStatusAsync(Guid id, AdopterStatus status, CancellationToken token = default);
    Task<bool?> VerifyForShelterAsync(Guid id, Guid shelterId, CancellationToken token = default);
}
