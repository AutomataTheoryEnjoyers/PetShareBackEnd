using Database;
using Database.Entities;
using FluentAssertions;
using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;
using ShelterModule.Models;
using Xunit;

namespace ShelterModuleTests;

[Trait("Category", "Integration")]
public sealed class ShelterEndpointTests : IAsyncLifetime
{
    private readonly ShelterEntity _shelter = new()
    {
        Id = Guid.NewGuid(),
        Name = "test-shelter",
        IsAuthorized = false
    };

    private readonly IntegrationTestSetup _testSetup = new();

    public async Task InitializeAsync()
    {
        await using var context = _testSetup.Services.GetRequiredService<PetShareDbContext>();
        context.Shelters.Add(_shelter);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldReturnAllShelters()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelters").GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<IEnumerable<ShelterResponse>>();
        shelters.Should().
                 BeEquivalentTo(new[]
                 {
                     new ShelterResponse
                     {
                         Id = _shelter.Id,
                         Name = _shelter.Name,
                         IsAuthorized = _shelter.IsAuthorized
                     }
                 });
    }

    [Fact]
    public async Task ShouldReturnShelterById()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelters", _shelter.Id).GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<ShelterResponse>();
        shelters.Should().
                 BeEquivalentTo(new ShelterResponse
                 {
                     Id = _shelter.Id,
                     Name = _shelter.Name,
                     IsAuthorized = _shelter.IsAuthorized
                 });
    }

    [Fact]
    public async Task ShouldAddShelter()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelters").
                                    PostJsonAsync(new ShelterCreationRequest
                                    {
                                        Name = "new-shelter"
                                    });
        response.StatusCode.Should().Be(200);
        var newShelter = await response.GetJsonAsync<ShelterResponse>();
        newShelter.Should().
                   BeEquivalentTo(new ShelterResponse
                   {
                       Id = Guid.NewGuid(),
                       Name = "new-shelter",
                       IsAuthorized = false
                   }, options => options.Excluding(s => s.Id));

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Should().
                ContainEquivalentOf(new ShelterEntity
                {
                    Id = newShelter.Id,
                    Name = "new-shelter",
                    IsAuthorized = false
                });
    }

    [Fact]
    public async Task ShouldAuthorizeShelter()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelters", _shelter.Id).
                                    PutJsonAsync(new ShelterAuthorizationRequest
                                    {
                                        IsAuthorized = true
                                    });
        response.StatusCode.Should().Be(200);
        var newShelter = await response.GetJsonAsync<ShelterResponse>();
        newShelter.Should().
                   BeEquivalentTo(new ShelterResponse
                   {
                       Id = _shelter.Id,
                       Name = _shelter.Name,
                       IsAuthorized = true
                   });

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Single(e => e.Id == _shelter.Id).
                Should().
                BeEquivalentTo(new ShelterEntity
                {
                    Id = _shelter.Id,
                    Name = _shelter.Name,
                    IsAuthorized = true
                });
    }
}
