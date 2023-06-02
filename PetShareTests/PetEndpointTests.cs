using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PetShare.Controllers;
using PetShare.Models;
using PetShare.Models.Pets;
using PetShare.Models.Shelters;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Integration")]
public sealed class PetEndpointTests : IAsyncLifetime
{
    private readonly PetEntity[] _pets;
    private readonly ShelterEntity[] _shelters;

    private readonly IntegrationTestSetup _testSetup = new();

    public PetEndpointTests()
    {
        _shelters = new ShelterEntity[]
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

        _pets = new PetEntity[]
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "test-pet1",
                Breed = "test-breed",
                Species = "test-species",
                Birthday = DateTime.Now,
                Description = "test-description1",
                Photo = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png",
                ShelterId = _shelters[0].Id,
                Sex = PetSex.Unknown,
                Status = PetStatus.Active
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "test-pet2",
                Breed = "test-breed",
                Species = "test-species",
                Birthday = DateTime.Now,
                Description = "test-description2",
                Photo = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png",
                ShelterId = _shelters[0].Id,
                Sex = PetSex.Unknown,
                Status = PetStatus.Active
            },
        };
    }

    public async Task InitializeAsync()
    {
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.AddRange(_shelters);
        context.Pets.AddRange(_pets);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _testSetup.DisposeAsync();
    }

    [Fact]
    public async Task GetShouldFetchPetsFromShelter()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("shelter", "pets").GetAsync();
        response.StatusCode.Should().Be(200);
        var pets = await response.GetJsonAsync<PaginatedPetsResponse>();
        pets.Should().
             BeEquivalentTo(new PaginatedPetsResponse
             {
                 Pets = _pets.Select(p => Pet.FromEntity(p).ToResponse()).ToList(),
                 PageNumber = 0,
                 Count = 2
             });
    }

    [Fact]
    public async Task GetShouldFailWithWrongPaginationParams()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var query = new PaginationQueryRequest
        {
            PageCount = 10,
            PageNumber = 10,
        };
        var response = await client.Request("shelter","pets").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetShouldFetchPaginatedFragmentOfShelters()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var query = new PaginationQueryRequest
        {
            PageCount = 1,
            PageNumber = 0,
        };
        var response = await client.Request("shelter","pets").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var pets = await response.GetJsonAsync<PaginatedPetsResponse>();
        pets.Count.Should().Be(2);
        pets.PageNumber.Should().Be(0);
        pets.Pets.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetShouldFetchPaginatedEndFragmentOfShelters()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var query = new PaginationQueryRequest
        {
            PageCount = 1,
            PageNumber = 1,
        };
        var response = await client.Request("shelter", "pets").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var pets = await response.GetJsonAsync<PaginatedPetsResponse>();
        pets.Count.Should().Be(2);
        pets.PageNumber.Should().Be(1);
        pets.Pets.Count.Should().Be(1);
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
        var pets = await response.GetJsonAsync<PaginatedPetsResponse>();
        pets.Pets.Should().BeEmpty();
        pets.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetShouldFetchPetById()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet", _pets[0].Id).GetAsync();
        response.StatusCode.Should().Be(200);
        var shelters = await response.GetJsonAsync<PetResponse>();
        shelters.Should().
                 BeEquivalentTo(new PetResponse
                 {
                     Id = _pets[0].Id,
                     Name = _pets[0].Name,
                     Species = _pets[0].Species,
                     Breed = _pets[0].Breed,
                     Birthday = _pets[0].Birthday,
                     Description = _pets[0].Description,
                     PhotoUrl = _pets[0].Photo,
                     Shelter = Shelter.FromEntity(_pets[0].Shelter).ToResponse(),
                     Status = _pets[0].Status.ToString(),
                     Sex = _pets[0].Sex.ToString()
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
                  Id = petId
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
                  Id = wrongId
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
            PhotoUrl = "https://www.londrinatur.com.br/wp-content/uploads/2020/04/pets-header.png"
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
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
                   Shelter = Shelter.FromEntity(_shelters[0]).ToResponse(),
                   Status = _pets[0].Status.ToString(),
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
                    ShelterId = _shelters[0].Id,
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
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("pet", _pets[0].Id).PutJsonAsync(request);
        response.StatusCode.Should().Be(200);
        var updatedPet = await response.GetJsonAsync<PetResponse>();
        updatedPet.Should().
                   BeEquivalentTo(new PetResponse
                   {
                       Id = _pets[0].Id,
                       Name = request.Name,
                       Breed = request.Breed,
                       Species = _pets[0].Species,
                       Birthday = _pets[0].Birthday,
                       Description = request.Description,
                       Shelter = Shelter.FromEntity(_pets[0].Shelter).ToResponse(),
                       Status = _pets[0].Status.ToString(),
                       PhotoUrl = _pets[0].Photo,
                       Sex = _pets[0].Sex.ToString()
                   });

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Pets.Single(e => e.Id == _pets[0].Id).
                Should().
                BeEquivalentTo(new PetEntity
                {
                    Id = _pets[0].Id,
                    Name = request.Name,
                    Breed = request.Breed,
                    Species = _pets[0].Species,
                    Birthday = _pets[0].Birthday,
                    Description = request.Description,
                    Photo = _pets[0].Photo,
                    ShelterId = _shelters[0].Id,
                    Sex = _pets[0].Sex,
                    Status = _pets[0].Status
                });
    }

    [Fact]
    public async Task PutShouldDeletePet()
    {
        var request = new PetUpdateRequest
        {
            Status = "Deleted"
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("pet", _pets[0].Id).PutJsonAsync(request);
        response.StatusCode.Should().Be(200);
        var updatedPet = await response.GetJsonAsync<PetResponse>();
        updatedPet.Should().
                   BeEquivalentTo(new PetResponse
                   {
                       Id = _pets[0].Id,
                       Name = _pets[0].Name,
                       Breed = _pets[0].Breed,
                       Species = _pets[0].Species,
                       Birthday = _pets[0].Birthday,
                       Description = _pets[0].Description,
                       Shelter = Shelter.FromEntity(_pets[0].Shelter).ToResponse(),
                       Status = PetStatus.Deleted.ToString(),
                       PhotoUrl = _pets[0].Photo,
                       Sex = _pets[0].Sex.ToString()
                   });

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Pets.Single(e => e.Id == _pets[0].Id).
                Should().
                BeEquivalentTo(new PetEntity
                {
                    Id = _pets[0].Id,
                    Name = _pets[0].Name,
                    Breed = _pets[0].Breed,
                    Species = _pets[0].Species,
                    Birthday = _pets[0].Birthday,
                    Description = _pets[0].Description,
                    Photo = _pets[0].Photo,
                    ShelterId = _shelters[0].Id,
                    Sex = _pets[0].Sex,
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
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("pet", wrongId).PutJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  ResourceName = nameof(Pet),
                  Id = wrongId
              });
    }

    [Fact]
    public async Task PostPhotoShouldUpdatePetPhoto()
    {
        var stream = new MemoryStream("photo content"u8.ToArray());
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id);
        var response = await client.Request("pet", _pets[0].Id, "photo").
                                    PostMultipartAsync(mp => mp.AddFile("file", stream, "new-photo.jpg", "image/jpg"));
        response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Pets.Single(pet => pet.Id == _pets[0].Id).Photo.Should().Be("photo.jpg");
    }
}
