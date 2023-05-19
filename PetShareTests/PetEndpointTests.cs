using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PetShare;
using PetShare.Controllers;
using PetShare.Models.Pets;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Integration")]
public sealed class PetEndpointTests : IAsyncLifetime
{
    private readonly PetEntity _pet;
    private readonly ShelterEntity _shelter;

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
            Photo = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png",
            ShelterId = _shelter.Id,
            Sex = PetSex.Unknown,
            Status = PetStatus.Active
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
    public async Task GetShouldFetchPetsFromShelter()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
        var response = await client.Request("shelter", "pets").GetAsync();
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
                     PhotoUrl = _pet.Photo,
                     Shelter = Shelter.FromEntity(_pet.Shelter).ToResponse(),
                     Status = _pet.Status.ToString(),
                     Sex = _pet.Sex.ToString()
                 }
             });
    }

    [Fact]
    public async Task GetShouldNotReturnDeletedPets()
    {
        var shelterId = Guid.NewGuid();
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Add(new ShelterEntity
        {
            Id = shelterId,
            Email = "some@email.com",
            PhoneNumber = "123456789",
            UserName = "shel-ter",
            FullShelterName = "Shel Ter",
            IsAuthorized = true,
            Address = new Address
            {
                City = "city city",
                Country = "country country",
                PostalCode = "12-345",
                Province = "province",
                Street = "long street"
            }
        });
        context.Pets.Add(new PetEntity
        {
            Id = Guid.NewGuid(),
            ShelterId = shelterId,
            Name = "name",
            Breed = "breed",
            Species = "species",
            Description = "description",
            Sex = PetSex.DoesNotApply,
            Birthday = DateTime.Today,
            Photo = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png",
            Status = PetStatus.Deleted
        });
        await context.SaveChangesAsync();

        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, shelterId).AllowAnyHttpStatus();
        var response = await client.Request("shelter", "pets").GetAsync();
        response.StatusCode.Should().Be(200);
        var pets = await response.GetJsonAsync<IEnumerable<PetResponse>>();
        pets.Should().BeEmpty();
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
                     PhotoUrl = _pet.Photo,
                     Shelter = Shelter.FromEntity(_pet.Shelter).ToResponse(),
                     Status = _pet.Status.ToString(),
                     Sex = _pet.Sex.ToString()
                 });
    }

    [Fact]
    public async Task ShouldNotReturnDeletedPetsById()
    {
        var shelterId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Add(new ShelterEntity
        {
            Id = shelterId,
            Email = "some@email.com",
            PhoneNumber = "123456789",
            UserName = "shel-ter",
            FullShelterName = "Shel Ter",
            IsAuthorized = true,
            Address = new Address
            {
                City = "city city",
                Country = "country country",
                PostalCode = "12-345",
                Province = "province",
                Street = "long street"
            }
        });
        context.Pets.Add(new PetEntity
        {
            Id = petId,
            ShelterId = shelterId,
            Name = "name",
            Breed = "breed",
            Species = "species",
            Description = "description",
            Sex = PetSex.DoesNotApply,
            Birthday = DateTime.Today,
            Photo = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png",
            Status = PetStatus.Deleted
        });
        await context.SaveChangesAsync();

        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet", petId).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  ResourceName = nameof(Pet),
                  Id = petId.ToString()
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
        var request = new PetCreationRequest
        {
            Name = "test-pet2",
            Breed = "test-breed2",
            Species = "test-species2",
            Birthday = DateTime.Now,
            Description = "test-description2",
            Sex = "Female",
            PhotoUrl = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png",
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
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
                   PhotoUrl = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png",
                   Shelter = Shelter.FromEntity(_shelter).ToResponse(),
                   Status = _pet.Status.ToString(),
                   Sex = request.Sex
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
                    ShelterId = _shelter.Id,
                    Photo = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png",
                    Sex = PetSex.Female,
                    Status = PetStatus.Active
                });
    }

    [Fact]
    public async Task PutShouldUpdatePet()
    {
        var request = new PetUpdateRequest
        {
            Name = "test-pet-updated",
            Breed = "test-breed-updated",
            Description = "test-description-updated"
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
        var response = await client.Request("pet", _pet.Id).PutJsonAsync(request);
        response.StatusCode.Should().Be(200);
        var updatedPet = await response.GetJsonAsync<PetResponse>();
        updatedPet.Should().
                   BeEquivalentTo(new PetResponse
                   {
                       Id = _pet.Id,
                       Name = request.Name,
                       Breed = request.Breed,
                       Species = _pet.Species,
                       Birthday = _pet.Birthday,
                       Description = request.Description,
                       Shelter = Shelter.FromEntity(_pet.Shelter).ToResponse(),
                       Status = _pet.Status.ToString(),
                       PhotoUrl = _pet.Photo,
                       Sex = _pet.Sex.ToString()
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
                    Species = _pet.Species,
                    Birthday = _pet.Birthday,
                    Description = request.Description,
                    Photo = _pet.Photo,
                    ShelterId = _shelter.Id,
                    Sex = _pet.Sex,
                    Status = _pet.Status
                });
    }

    [Fact]
    public async Task PutShouldDeletePet()
    {
        var request = new PetUpdateRequest
        {
            Status = "Deleted"
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
        var response = await client.Request("pet", _pet.Id).PutJsonAsync(request);
        response.StatusCode.Should().Be(200);
        var updatedPet = await response.GetJsonAsync<PetResponse>();
        updatedPet.Should().
                   BeEquivalentTo(new PetResponse
                   {
                       Id = _pet.Id,
                       Name = _pet.Name,
                       Breed = _pet.Breed,
                       Species = _pet.Species,
                       Birthday = _pet.Birthday,
                       Description = _pet.Description,
                       Shelter = Shelter.FromEntity(_pet.Shelter).ToResponse(),
                       Status = PetStatus.Deleted.ToString(),
                       PhotoUrl = _pet.Photo,
                       Sex = _pet.Sex.ToString()
                   });

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Pets.Single(e => e.Id == _pet.Id).
                Should().
                BeEquivalentTo(new PetEntity
                {
                    Id = _pet.Id,
                    Name = _pet.Name,
                    Breed = _pet.Breed,
                    Species = _pet.Species,
                    Birthday = _pet.Birthday,
                    Description = _pet.Description,
                    Photo = _pet.Photo,
                    ShelterId = _shelter.Id,
                    Sex = _pet.Sex,
                    Status = PetStatus.Deleted
                });
    }

    [Fact]
    public async Task PutShouldFailWithWrongPetId()
    {
        var wrongId = Guid.NewGuid();
        var request = new PetUpdateRequest
        {
            Name = "test-pet2",
            Breed = "test-breed2"
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
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

    [Fact]
    public async Task PostPhotoShouldUpdatePetPhoto()
    {
        var stream = new MemoryStream("photo content"u8.ToArray());
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id);
        var response = await client.Request("pet", _pet.Id, "photo").
                                    PostMultipartAsync(mp => mp.AddFile("file", stream, "new-photo.jpg", "image/jpg"));
        response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Pets.Single(pet => pet.Id == _pet.Id).Photo.Should().Be("photo.jpg");
    }
}
