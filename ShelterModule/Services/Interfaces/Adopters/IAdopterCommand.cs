using ShelterModule.Models.Adopters;

namespace ShelterModule.Services.Interfaces.Adopters;

public interface IAdopterCommand
{
    Task AddAsync(Adopter adopter);
    Task<Adopter?> VerifyAsync(Guid id, bool verificationStatus);
}
