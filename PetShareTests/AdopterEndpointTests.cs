using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PetShare.Controllers;
using PetShare.Models;
using PetShare.Models.Adopters;
using PetShare.Models.Shelters;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Integration")]
public sealed class AdopterEndpointTests : IAsyncLifetime
{
    private readonly AdopterEntity[] _adopters = new AdopterEntity[]
    {
        new()
        {
            Id = Guid.NewGuid(),
            UserName = "test-adopter1",
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
        },
        new()
        {
            Id = Guid.NewGuid(),
            UserName = "test-adopter2",
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
        },
    };

    private readonly ShelterEntity[] _shelters = new ShelterEntity[]
    {
            new ()
            {
                Id = Guid.NewGuid(),
                UserName = "shelter1",
                Email = "mail1@mail.mail",
                PhoneNumber = "123456789",
                FullShelterName = "Shelter 1",
                IsAuthorized = null,
                Address = new Address
                {
                    Country = "country",
                    Province = "province",
                    City = "city",
                    Street = "street",
                    PostalCode = "12-345"
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "shelter2",
                Email = "mail2@mail.mail",
                PhoneNumber = "123123123",
                FullShelterName = "Shelter 2",
                IsAuthorized = null,
                Address = new Address
                {
                    Country = "country",
                    Province = "province",
                    City = "city",
                    Street = "street",
                    PostalCode = "54-321"
                }
            }
    };

    private readonly IntegrationTestSetup _testSetup = new();

    public async Task InitializeAsync()
    {
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.AddRange(_shelters);
        context.Adopters.AddRange(_adopters);
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
        var adopters = await client.Request("adopter").GetJsonAsync<PaginatedAdoptersResponse>();
        adopters.Should().
                 BeEquivalentTo(new PaginatedAdoptersResponse
                 {
                     Adopters = _adopters.Select(a => Adopter.FromEntity(a).ToResponse()).ToList(),
                     PageNumber = 0,
                     Count = 2,
                 });
    }

    [Fact]
    public async Task GetShouldFailWithWrongPaginationParams()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus().WithAuth(Roles.Admin);
        var response = await client.Request("adopter").SetQueryParams(new { PageCount = -1, PageNumber = -1 }).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetShouldFetchPaginatedFragmentOfShelters()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Admin);
        var response = await client.Request("adopter").SetQueryParams(new { PageCount = 1, PageNumber = 0 }).GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<PaginatedAdoptersResponse>();
        shelters.Count.Should().Be(2);
        shelters.PageNumber.Should().Be(0);
        shelters.Adopters.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetShouldFetchPaginatedEndFragmentOfShelters()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Admin);
        var response = await client.Request("adopter").SetQueryParams(new { PageCount = 1, PageNumber = 1 }).GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<PaginatedAdoptersResponse>();
        shelters.Count.Should().Be(2);
        shelters.PageNumber.Should().Be(1);
        shelters.Adopters.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetShouldFetchAdopterById()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Adopter, _adopters[0].Id);
        var adopter = await client.Request("adopter", _adopters[0].Id).GetJsonAsync<AdopterResponse>();
        adopter.Should().
                BeEquivalentTo(new AdopterResponse
                {
                    Id = _adopters[0].Id,
                    UserName = "test-adopter1",
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
        var updatedAdopter = await (await client.Request("adopter", _adopters[0].Id).
                                                 PutJsonAsync(new
                                                 {
                                                     status = (int)AdopterStatus.Blocked
                                                 })).GetJsonAsync<AdopterResponse>();

        updatedAdopter.Should().
                       BeEquivalentTo(new AdopterResponse
                       {
                           Id = _adopters[0].Id,
                           UserName = "test-adopter1",
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
        context.Adopters.Single(e => e.Id == _adopters[0].Id).Status.Should().Be(AdopterStatus.Blocked);
    }

    [Fact]
    public async Task GetShouldCheckIfAdopterIsVerified()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id);
        var response1 = await client.Request("adopter", _adopters[0].Id, "isVerified").
                                     GetJsonAsync<bool>();
        response1.Should().BeFalse();

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Verifications.Add(new AdopterVerificationEntity
        {
            AdopterId = _adopters[0].Id,
            ShelterId = _shelters[0].Id
        });
        await context.SaveChangesAsync();

        var response2 = await client.Request("adopter", _adopters[0].Id, "isVerified").
                                     GetJsonAsync<bool>();
        response2.Should().BeTrue();
    }

    [Fact]
    public async Task PutShouldVerifyAdopter()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("adopter", _adopters[0].Id, "verify").PutAsync();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Verifications.Should().
                ContainEquivalentOf(new AdopterVerificationEntity
                {
                    AdopterId = _adopters[0].Id,
                    ShelterId = _shelters[0].Id
                });
    }
}
