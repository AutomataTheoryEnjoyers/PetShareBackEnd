using PetShare.Models.Applications;
using PetShare.Results;

namespace PetShare.Services.Interfaces.Applications;

public interface IApplicationCommand
{
    Task<Result<Application>> CreateAsync(Guid announcementId, Guid adopterId, CancellationToken token = default);
    Task<Result<Application>> WithdrawAsync(Guid id, CancellationToken token = default);
    Task<Result<Application>> AcceptAsync(Guid id, CancellationToken token = default);
    Task<Result<Application>> RejectAsync(Guid id, CancellationToken token = default);
}
