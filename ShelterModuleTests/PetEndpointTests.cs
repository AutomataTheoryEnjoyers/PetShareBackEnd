using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ShelterModuleTests
{
    public sealed class PetEndpointTests : IAsyncLifetime
    {
        private readonly static Guid _shelterId = Guid.NewGuid();

        private readonly ShelterEntity _shelter = new()
        {
            Id = _shelterId,
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

        private readonly PetEnitiy _pet = new()
        {
            Id = Guid.NewGuid(),
            Name = "test-pet",
            Breed = "test-breed",
            Species = "test-species",
            Birthday = DateTime.Now,
            Description = "test-escription",
            Photo = "test-photo",
            ShelterId = _shelterId,
        };

        private readonly IntegrationTestSetup _testSetup = new();

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
            //_testSetup.Dispose();
            await Task.CompletedTask;
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
                        ShelterId = _pet.ShelterId
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
                BeEquivalentTo( 
                    new PetResponse
                    {
                        Id = _pet.Id,
                        Name = _pet.Name,
                        Species = _pet.Species,
                        Breed = _pet.Breed,
                        Birthday = _pet.Birthday,
                        Description = _pet.Description,
                        Photo = _pet.Photo,
                        ShelterId = _pet.ShelterId
                    });
        }

        [Fact]
        public async Task GetShouldFailWithWrongPetId()
        {
            Guid wrongId = Guid.NewGuid();
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("pet", wrongId).GetAsync();
            response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
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
                ShelterId = _shelterId,
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
                          ShelterId = request.ShelterId
                       }, options => options.Excluding(s => s.Id));

            using var scope = _testSetup.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
            context.Pets.Should().
                ContainEquivalentOf(new PetEnitiy
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
            Guid wrongShelterId = Guid.NewGuid();
            var request = new PetUpsertRequest
            {
                Name = "test-pet2",
                Breed = "test-breed2",
                Species = "test-species2",
                Birthday = DateTime.Now,
                Description = "test-description2",
                Photo = "test-photo2",
                ShelterId = wrongShelterId,
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
                Description = "test-escription-updated",
                Photo = "test-photo-updated",
                ShelterId = _shelterId,
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
                    ShelterId = request.ShelterId
                });

            using var scope = _testSetup.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
            context.Pets.Single(e => e.Id == _pet.Id)
                .Should().BeEquivalentTo(new PetEnitiy
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
            Guid wrongShelterId = Guid.NewGuid();
            var request = new PetUpsertRequest
            {
                Name = "test-pet2",
                Breed = "test-breed2",
                Species = "test-species2",
                Birthday = DateTime.Now,
                Description = "test-description2",
                Photo = "test-photo2",
                ShelterId = wrongShelterId,
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("pet",_pet.Id).PutJsonAsync(request);
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task PutShouldFailWithWrongPetId()
        {
            Guid wrongrId = Guid.NewGuid();
            var request = new PetUpsertRequest
            {
                Name = "test-pet2",
                Breed = "test-breed2",
                Species = "test-species2",
                Birthday = DateTime.Now,
                Description = "test-description2",
                Photo = "test-photo2",
                ShelterId = _shelterId,
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("pet", wrongrId).PutJsonAsync(request);
            response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
