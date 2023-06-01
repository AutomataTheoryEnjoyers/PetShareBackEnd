using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PetShare.Controllers;
using PetShare.Models.Adopters;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Integration")]
public sealed class AdopterEndpointTests : IAsyncLifetime
{
    private readonly AdopterEntity _adopter = new()
    {
        Id = Guid.NewGuid(),
        UserName = "test-adopter",
        Email = "mail@mail.mail",
        PhoneNumber = "123456789",
        Status = AdopterStatus.Active,
        Address = new Address
        {
            Country = "test-country",
            Province = "test-province",
            City = "test-city",
            Street = "test-street",
            PostalCode = "test-postalCode"
        }
    };

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
            PostalCode = "test-postalCode"
        }
    };

    private readonly IntegrationTestSetup _testSetup = new();

    public async Task InitializeAsync()
    {
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Add(_shelter);
        context.Adopters.Add(_adopter);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _testSetup.DisposeAsync();
    }

    [Fact]
    public async Task GetShouldReturnAllAdopters()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Admin);
        var adopters = await client.Request("adopter").GetJsonAsync<IEnumerable<AdopterResponse>>();
        adopters.Should().
                 BeEquivalentTo(new[]
                 {
                     new AdopterResponse
                     {
                         Id = _adopter.Id,
                         UserName = "test-adopter",
                         Email = "mail@mail.mail",
                         PhoneNumber = "123456789",
                         Status = AdopterStatus.Active,
                         Address = new Address
                         {
                             Country = "test-country",
                             Province = "test-province",
                             City = "test-city",
                             Street = "test-street",
                             PostalCode = "test-postalCode"
                         }
                     }
                 });
    }

    [Fact]
    public async Task GetShouldFetchAdopterById()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Adopter, _adopter.Id);
        var adopter = await client.Request("adopter", _adopter.Id).GetJsonAsync<AdopterResponse>();
        adopter.Should().
                BeEquivalentTo(new AdopterResponse
                {
                    Id = _adopter.Id,
                    UserName = "test-adopter",
                    Email = "mail@mail.mail",
                    PhoneNumber = "123456789",
                    Status = AdopterStatus.Active,
                    Address = new Address
                    {
                        Country = "test-country",
                        Province = "test-province",
                        City = "test-city",
                        Street = "test-street",
                        PostalCode = "test-postalCode"
                    }
                });
    }

    [Fact]
    public async Task PostShouldCreateNewAdopter()
    {
        var request = new AdopterRequest
        {
            UserName = "new-adopter",
            Email = "new@adopter.com",
            PhoneNumber = "987654321",
            Address = new Address
            {
                Country = "test-country",
                Province = "test-province",
                City = "test-city",
                Street = "test-street",
                PostalCode = "test-postalCode"
            }
        };

        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Unassigned);
        var newAdopter =
            await (await client.Request("adopter").PostJsonAsync(request)).GetJsonAsync<AdopterResponse>();
        newAdopter.Should().
                   BeEquivalentTo(new AdopterResponse
                   {
                       Id = Guid.NewGuid(),
                       UserName = request.UserName,
                       Email = request.Email,
                       PhoneNumber = request.PhoneNumber,
                       Address = request.Address,
                       Status = AdopterStatus.Active
                   }, options => options.Excluding(adopter => adopter.Id));

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Adopters.Should().
                ContainSingle(e => e.Id == newAdopter.Id).
                Which.Should().
                BeEquivalentTo(new AdopterEntity
                {
                    Id = newAdopter.Id,
                    UserName = request.UserName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    Status = AdopterStatus.Active
                });
    }

    [Fact]
    public async Task PutShouldChangeAdopterStatus()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Admin);
        var updatedAdopter = await (await client.Request("adopter", _adopter.Id).
                                                 PutJsonAsync(new
                                                 {
                                                     status = (int)AdopterStatus.Blocked
                                                 })).GetJsonAsync<AdopterResponse>();

        updatedAdopter.Should().
                       BeEquivalentTo(new AdopterResponse
                       {
                           Id = _adopter.Id,
                           UserName = "test-adopter",
                           Email = "mail@mail.mail",
                           PhoneNumber = "123456789",
                           Status = AdopterStatus.Blocked,
                           Address = new Address
                           {
                               Country = "test-country",
                               Province = "test-province",
                               City = "test-city",
                               Street = "test-street",
                               PostalCode = "test-postalCode"
                           }
                       });

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Adopters.Single(e => e.Id == _adopter.Id).Status.Should().Be(AdopterStatus.Blocked);
    }

    [Fact]
    public async Task GetShouldCheckIfAdopterIsVerified()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id);
        var response1 = await client.Request("adopter", _adopter.Id, "isVerified").
                                     GetJsonAsync<bool>();
        response1.Should().BeFalse();

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Verifications.Add(new AdopterVerificationEntity
        {
            AdopterId = _adopter.Id,
            ShelterId = _shelter.Id
        });
        await context.SaveChangesAsync();

        var response2 = await client.Request("adopter", _adopter.Id, "isVerified").
                                     GetJsonAsync<bool>();
        response2.Should().BeTrue();
    }

    [Fact]
    public async Task PutShouldVerifyAdopter()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
        var response = await client.Request("adopter", _adopter.Id, "verify").PutAsync();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Verifications.Should().
                ContainEquivalentOf(new AdopterVerificationEntity
                {
                    AdopterId = _adopter.Id,
                    ShelterId = _shelter.Id
                });
    }
}
