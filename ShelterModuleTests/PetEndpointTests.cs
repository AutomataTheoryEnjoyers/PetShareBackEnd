using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ShelterModule;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using Xunit;

namespace ShelterModuleTests;

[Trait("Category", "Integration")]
public sealed class PetEndpointTests : IAsyncLifetime
{
    private readonly PetEntity _pet;
    private readonly ShelterEntity _shelter;

    private readonly ShelterResponse _shelterResponse;

    private readonly IntegrationTestSetup _testSetup = new();

    public PetEndpointTests()
    {
        _shelter = new ShelterEntity
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
        _pet = new PetEntity
        {
            Id = Guid.NewGuid(),
            Name = "test-pet",
            Breed = "test-breed",
            Species = "test-species",
            Birthday = DateTime.Now,
            Description = "test-description",
            Photo = "test-photo",
            ShelterId = _shelter.Id
        };
        _shelterResponse = new ShelterResponse
        {
            Id = _shelter.Id,
            UserName = _shelter.UserName,
            FullShelterName = _shelter.FullShelterName,
            Email = _shelter.Email,
            PhoneNumber = _shelter.PhoneNumber,
            IsAuthorized = _shelter.IsAuthorized,
            Address = _shelter.Address
        };
    }

    public async Task InitializeAsync()
    {
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Add(_shelter);
        context.Pets.Add(_pet);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _testSetup.DisposeAsync();
    }

    [Fact]
    public async Task GetShouldFetchAllPets()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet").GetAsync();
        response.StatusCode.Should().Be(200);
        var pets = await response.GetJsonAsync<IEnumerable<PetResponse>>();
        pets.Should().
             BeEquivalentTo(new[]
             {
                 new PetResponse
                 {
                     Id = _pet.Id,
                     Name = _pet.Name,
                     Species = _pet.Species,
                     Breed = _pet.Breed,
                     Birthday = _pet.Birthday,
                     Description = _pet.Description,
                     Photo = _pet.Photo,
                     ShelterId = _pet.ShelterId,
                     Shelter = _shelterResponse,
                 }
             });
    }

    [Fact]
    public async Task GetShouldFetchPetById()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet", _pet.Id).GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<PetResponse>();
        shelters.Should().
                 BeEquivalentTo(new PetResponse
                 {
                     Id = _pet.Id,
                     Name = _pet.Name,
                     Species = _pet.Species,
                     Breed = _pet.Breed,
                     Birthday = _pet.Birthday,
                     Description = _pet.Description,
                     Photo = _pet.Photo,
                     ShelterId = _pet.ShelterId,
                     Shelter = _shelterResponse,
                 });
    }

    [Fact]
    public async Task GetShouldFailWithWrongPetId()
    {
        var wrongId = Guid.NewGuid();
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet", wrongId).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  ResourceName = nameof(Pet),
                  Id = wrongId.ToString()
              });
    }

    [Fact]
    public async Task PostShouldAddPet()
    {
        var request = new PetUpsertRequest
        {
            Name = "test-pet2",
            Breed = "test-breed2",
            Species = "test-species2",
            Birthday = DateTime.Now,
            Description = "test-description2",
            Photo = "test-photo2",
            ShelterId = _shelter.Id
        };
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet").PostJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        var newPet = await response.GetJsonAsync<PetResponse>();
        newPet.Should().
               BeEquivalentTo(new PetResponse
               {
                   Id = Guid.NewGuid(),
                   Name = request.Name,
                   Breed = request.Breed,
                   Species = request.Species,
                   Birthday = request.Birthday,
                   Description = request.Description,
                   Photo = request.Photo,
                   ShelterId = request.ShelterId,
                   Shelter = _shelterResponse
               }, options => options.Excluding(s => s.Id));

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Pets.Should().
                ContainEquivalentOf(new PetEntity
                {
                    Id = newPet.Id,
                    Name = request.Name,
                    Breed = request.Breed,
                    Species = request.Species,
                    Birthday = request.Birthday,
                    Description = request.Description,
                    Photo = request.Photo,
                    ShelterId = request.ShelterId
                });
    }

    [Fact]
    public async Task PostShouldFailWithWrongShelterId()
    {
        var wrongShelterId = Guid.NewGuid();
        var request = new PetUpsertRequest
        {
            Name = "test-pet2",
            Breed = "test-breed2",
            Species = "test-species2",
            Birthday = DateTime.Now,
            Description = "test-description2",
            Photo = "test-photo2",
            ShelterId = wrongShelterId
        };
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet").PostJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task PutShouldUpdatePet()
    {
        var request = new PetUpsertRequest
        {
            Name = "test-pet-updated",
            Breed = "test-breed-updated",
            Species = "test-species-updated",
            Birthday = DateTime.Now.AddDays(-3),
            Description = "test-description-updated",
            Photo = "test-photo-updated",
            ShelterId = _shelter.Id
        };
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet", _pet.Id).PutJsonAsync(request);
        response.StatusCode.Should().Be(200);
        var updatedPet = await response.GetJsonAsync<PetResponse>();
        updatedPet.Should().
                   BeEquivalentTo(new PetResponse
                   {
                       Id = _pet.Id,
                       Name = request.Name,
                       Breed = request.Breed,
                       Species = request.Species,
                       Birthday = request.Birthday,
                       Description = request.Description,
                       Photo = request.Photo,
                       ShelterId = request.ShelterId,
                       Shelter = _shelterResponse
                   });

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Pets.Single(e => e.Id == _pet.Id).
                Should().
                BeEquivalentTo(new PetEntity
                {
                    Id = _pet.Id,
                    Name = request.Name,
                    Breed = request.Breed,
                    Species = request.Species,
                    Birthday = request.Birthday,
                    Description = request.Description,
                    Photo = request.Photo,
                    ShelterId = request.ShelterId
                });
    }

    [Fact]
    public async Task PutShouldFailWithWrongShelterId()
    {
        var wrongShelterId = Guid.NewGuid();
        var request = new PetUpsertRequest
        {
            Name = "test-pet2",
            Breed = "test-breed2",
            Species = "test-species2",
            Birthday = DateTime.Now,
            Description = "test-description2",
            Photo = "test-photo2",
            ShelterId = wrongShelterId
        };
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet", _pet.Id).PutJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task PutShouldFailWithWrongPetId()
    {
        var wrongId = Guid.NewGuid();
        var request = new PetUpsertRequest
        {
            Name = "test-pet2",
            Breed = "test-breed2",
            Species = "test-species2",
            Birthday = DateTime.Now,
            Description = "test-description2",
            Photo = "test-photo2",
            ShelterId = _shelter.Id
        };
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet", wrongId).PutJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  ResourceName = nameof(Pet),
                  Id = wrongId.ToString()
              });
    }
}
