using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PetShare.Controllers;
using PetShare.Models;
using PetShare.Models.Announcements;
using PetShare.Models.Shelters;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Integration")]
public sealed class ShelterEndpointTests : IAsyncLifetime
{
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
        context.Set<ShelterEntity>().OrderBy(x => x.UserName);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _testSetup.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetShouldFetchAllShelters()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelter").GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<PaginatedSheltersResponse>();
        shelters.Should().
                BeEquivalentTo(new PaginatedSheltersResponse
                {
                    Shelters = _shelters.Select(s => Shelter.FromEntity(s).ToResponse()).ToArray(),
                    PageNumber = 0,
                    Count = 2
                });
    }

    [Fact]
    public async Task GetShouldFailWithWrongPaginationParams()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new PaginationQueryRequest
        {
            PageCount = -10,
            PageNumber = -10,
        };
        var response = await client.Request("shelter").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetShouldFetchPaginatedFragmentOfShelters()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new PaginationQueryRequest
        {
            PageCount = 1,
            PageNumber = 0,
        };
        var response = await client.Request("shelter").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<PaginatedSheltersResponse>();
        shelters.Count.Should().Be(2);
        shelters.PageNumber.Should().Be(0);
        shelters.Shelters.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetShouldFetchPaginatedEndFragmentOfShelters()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new PaginationQueryRequest
        {
            PageCount = 1,
            PageNumber = 1,
        };
        var response = await client.Request("shelter").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<PaginatedSheltersResponse>();
        shelters.Count.Should().Be(2);
        shelters.PageNumber.Should().Be(1);
        shelters.Shelters.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetShouldFetchShelterById()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelter", _shelters[0].Id).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        var shelters = await response.GetJsonAsync<ShelterResponse>();
        shelters.Should().
                 BeEquivalentTo(Shelter.FromEntity(_shelters[0]).ToResponse());
    }

    [Fact]
    public async Task GetShouldFailWithWrongShelterId()
    {
        var wrongId = Guid.NewGuid();
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("shelter", wrongId).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  ResourceName = nameof(Shelter),
                  Id = wrongId
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
                PostalCode = "test-postalCode"
            }
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Unassigned).AllowAnyHttpStatus();
        var response = await client.Request("shelter").PostJsonAsync(request);
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
                       IsAuthorized = true,
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
                    IsAuthorized = true,
                    Address = request.Address
                });
    }

    [Fact]
    public async Task PutShouldAuthorizeShelter()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Admin).AllowAnyHttpStatus();
        var response = await client.Request("shelter", _shelters[0].Id).
                                    PutJsonAsync(new ShelterAuthorizationRequest
                                    {
                                        IsAuthorized = true
                                    });
        response.StatusCode.Should().Be(200);
        var newShelter = await response.GetJsonAsync<ShelterResponse>();
        newShelter.Should().
                   BeEquivalentTo(new ShelterResponse
                   {
                       Id = _shelters[0].Id,
                       UserName = _shelters[0].UserName,
                       FullShelterName = _shelters[0].FullShelterName,
                       Email = _shelters[0].Email,
                       PhoneNumber = _shelters[0].PhoneNumber,
                       IsAuthorized = true,
                       Address = _shelters[0].Address
                   });

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Single(e => e.Id == _shelters[0].Id).
                Should().
                BeEquivalentTo(new ShelterEntity
                {
                    Id = _shelters[0].Id,
                    UserName = _shelters[0].UserName,
                    FullShelterName = _shelters[0].FullShelterName,
                    Email = _shelters[0].Email,
                    PhoneNumber = _shelters[0].PhoneNumber,
                    IsAuthorized = true,
                    Address = _shelters[0].Address
                });
    }

    [Fact]
    public async Task PutShouldFailWithWrongShelterId()
    {
        var wrongId = Guid.NewGuid();
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Admin).AllowAnyHttpStatus();
        var response = await client.Request("shelter", wrongId).
                                    PutJsonAsync(new ShelterAuthorizationRequest
                                    {
                                        IsAuthorized = true
                                    });
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  ResourceName = nameof(Shelter),
                  Id = wrongId
              });
    }
}
