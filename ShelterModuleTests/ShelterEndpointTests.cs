using Database;
using Database.Entities;
using FluentAssertions;
using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;
using ShelterModule.Models.Shelters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.ValueObjects;
using Xunit;
using System.Linq;

namespace ShelterModuleTests;

[Trait("Category", "Integration")]
public sealed class ShelterEndpointTests : IAsyncLifetime
{
    private readonly ShelterEntity _shelter = new()
    {
        Id = Guid.NewGuid(),
        UserName = "test-shelter",
        Email = "mail@mail.mail",
        PhoneNumber = "123456789",
        FullShelterName = "Test Shelter",
        IsAuthorized = null,
        Address = new Address
        {
            Country = "test-country",
            Province = "test-province",
            City = "test-city",
            Street = "test-street",
            PostalCode = "test-postalCode",
        }
    };

    private readonly IntegrationTestSetup _testSetup = new();

    public async Task InitializeAsync()
    {
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Add(_shelter);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetShouldFetchAllShelters()
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
                         UserName = _shelter.UserName,
                         FullShelterName = _shelter.FullShelterName,
                         Email = _shelter.Email,
                         PhoneNumber = _shelter.PhoneNumber,
                         IsAuthorized = _shelter.IsAuthorized,
                         Address = _shelter.Address
                     }
                 });
    }

    [Fact]
    public async Task GetShouldFetchShelterById()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelters", _shelter.Id).GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<ShelterResponse>();
        shelters.Should().
                 BeEquivalentTo(new ShelterResponse
                 {
                     Id = _shelter.Id,
                     UserName = _shelter.UserName,
                     FullShelterName = _shelter.FullShelterName,
                     Email = _shelter.Email,
                     PhoneNumber = _shelter.PhoneNumber,
                     IsAuthorized = _shelter.IsAuthorized,
                     Address = _shelter.Address
                 });
    }

    [Fact]
    public async Task PostShouldAddShelter()
    {
        var request = new ShelterCreationRequest
        {
            UserName = "new-shelter",
            FullShelterName = "New Shelter",
            Email = "cool@website.com",
            PhoneNumber = "987654321",
            Address = new Address
            {
                Country = "test-country",
                Province = "test-province",
                City = "test-city",
                Street = "test-street",
                PostalCode = "test-postalCode",
            }
        };
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelters").PostJsonAsync(request);
        response.StatusCode.Should().Be(200);
        var newShelter = await response.GetJsonAsync<ShelterResponse>();
        newShelter.Should().
                   BeEquivalentTo(new ShelterResponse
                   {
                       Id = Guid.NewGuid(),
                       UserName = request.UserName,
                       FullShelterName = request.FullShelterName,
                       Email = request.Email,
                       PhoneNumber = request.PhoneNumber,
                       IsAuthorized = null,
                       Address = request.Address
                   }, options => options.Excluding(s => s.Id));

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Should().
                ContainEquivalentOf(new ShelterEntity
                {
                    Id = newShelter.Id,
                    UserName = request.UserName,
                    FullShelterName = request.FullShelterName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    IsAuthorized = null,
                    Address = request.Address
                });
    }

    [Fact]
    public async Task PutShouldAuthorizeShelter()
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
                       UserName = _shelter.UserName,
                       FullShelterName = _shelter.FullShelterName,
                       Email = _shelter.Email,
                       PhoneNumber = _shelter.PhoneNumber,
                       IsAuthorized = true,
                       Address = _shelter.Address
                   });

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Single(e => e.Id == _shelter.Id).
                Should().
                BeEquivalentTo(new ShelterEntity
                {
                    Id = _shelter.Id,
                    UserName = _shelter.UserName,
                    FullShelterName = _shelter.FullShelterName,
                    Email = _shelter.Email,
                    PhoneNumber = _shelter.PhoneNumber,
                    IsAuthorized = true,
                    Address = _shelter.Address
                });
    }
}
