using Microsoft.Extensions.Options;
using PetShare.Configuration;
using PetShare.Services.Interfaces.Adopters;

namespace PetShare.Services;

public sealed class PeriodicAdopterDeletion : BackgroundService
{
    private readonly IOptions<AdopterDeletionConfiguration> _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public PeriodicAdopterDeletion(IServiceScopeFactory factory,
        IOptions<AdopterDeletionConfiguration> configuration)
    {
        _scopeFactory = factory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(_configuration.Value.DeletionPeriod);
        while (!token.IsCancellationRequested)
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var command = scope.ServiceProvider.GetRequiredService<IAdopterCommand>();
                var limit = DateTime.Now - TimeSpan.FromDays(_configuration.Value.RetentionDays);
                await command.RemoveDeletedAsync(limit, token);
                await timer.WaitForNextTickAsync(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
    }
}
