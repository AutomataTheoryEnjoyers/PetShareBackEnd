using Database.Entities;
using ShelterModule.Models.Adopters;

namespace ShelterModule.Services.Interfaces.Adopters;

public interface IAdopterCommand
{
    Task AddAsync(Adopter adopter);
    Task<Adopter?> SetStatusAsync(Guid id, AdopterStatus status);
    Task<bool?> VerifyForShelterAsync(Guid id, Guid shelterId);
}
