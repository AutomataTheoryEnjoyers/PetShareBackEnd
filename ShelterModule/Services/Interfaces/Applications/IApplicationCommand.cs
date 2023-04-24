using ShelterModule.Models.Applications;

namespace ShelterModule.Services.Interfaces.Applications;

public interface IApplicationCommand
{
    Task<Application?> CreateAsync(Guid announcementId, Guid adopterId, CancellationToken token = default);
    Task<Application?> WithdrawAsync(Guid applicationId, CancellationToken token = default);
    Task<Application?> AcceptAsync(Guid applicationId, CancellationToken token = default);
    Task<Application?> RejectAsync(Guid applicationId, CancellationToken token = default);
}
