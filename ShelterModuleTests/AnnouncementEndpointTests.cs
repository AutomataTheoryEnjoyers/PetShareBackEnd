using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ShelterModule.Models.Announcements;
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
    public sealed class AnnouncementEndpointTests : IAsyncLifetime
    {
        private readonly static Guid _shelterId = Guid.NewGuid();

        private readonly static Guid _petId = Guid.NewGuid();
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
            Id = _petId,
            Name = "test-pet",
            Breed = "test-breed",
            Species = "test-species",
            Birthday = DateTime.Now,
            Description = "test-escription",
            Photo = "test-photo",
            ShelterId = _shelterId,
        };

        private readonly AnnouncementEntity _announcement = new()
        {
            Id = Guid.NewGuid(),
            Title = "test-announcement",
            Description = "test-description",
            CreationDate = DateTime.Now,
            Status = 0,
            LastUpdateDate = DateTime.Now,
            
            ShelterId = _shelterId,
            PetId = _petId,
        };

        private readonly IntegrationTestSetup _testSetup = new();

        public async Task InitializeAsync()
        {
            using var scope = _testSetup.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
            context.Shelters.Add(_shelter);
            context.Pets.Add(_pet);
            context.Announcements.Add(_announcement);
            await context.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            //_testSetup.Dispose();
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetShouldFetchAllAnnouncements()
        {
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements").GetAsync();
            response.StatusCode.Should().Be(200);
            var pets = await response.GetJsonAsync<IEnumerable<AnnouncementResponse>>();
            pets.Should().
                BeEquivalentTo(new[]
                {
                    new AnnouncementResponse
                    {
                        Id = _announcement.Id,
                        Title = _announcement.Title,
                        Description = _announcement.Description,
                        CreationDate = _announcement.CreationDate,
                        ClosingDate = _announcement.ClosingDate,
                        Status = _announcement.Status,
                        LastUpdateDate = _announcement.LastUpdateDate,
                        AuthorId = _announcement.ShelterId, 
                        PetId = _announcement.PetId,
                    }
                });
        }
        [Fact]
        public async Task GetAnnouncementsWithFiltersEmpty()
        {
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var query = new GetAllAnnouncementsFilteredQuery
            {
                MinAge = 1000,
            };
            var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
            response.StatusCode.Should().Be(200);
            var pets = await response.GetJsonAsync<IEnumerable<AnnouncementResponse>>();
            pets.Should().BeEmpty();   
        }

        [Fact]
        public async Task GetAnnouncementsWithFiltersNonEmpty()
        {
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var query = new GetAllAnnouncementsFilteredQuery
            {
                MaxAge = 1000,
            };
            var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
            response.StatusCode.Should().Be(200);
            var shelters = await response.GetJsonAsync<IEnumerable<AnnouncementResponse>>();
            shelters.Should().
                 BeEquivalentTo(new[]
                {
                    new AnnouncementResponse
                    {
                        Id = _announcement.Id,
                        Title = _announcement.Title,
                        Description = _announcement.Description,
                        CreationDate = _announcement.CreationDate,
                        ClosingDate = _announcement.ClosingDate,
                        Status = _announcement.Status,
                        LastUpdateDate = _announcement.LastUpdateDate,
                        AuthorId = _announcement.ShelterId,
                        PetId = _announcement.PetId,
                    }
                });
        }
        [Fact]
        public async Task GetShouldFetchAnnouncementById()
        {
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements", _announcement.Id).GetAsync();
            response.StatusCode.Should().Be(200);
            var shelters = await response.GetJsonAsync<AnnouncementResponse>();
            shelters.Should().
                BeEquivalentTo(
                   new AnnouncementResponse
                   {
                       Id = _announcement.Id,
                       Title = _announcement.Title,
                       Description = _announcement.Description,
                       CreationDate = _announcement.CreationDate,
                       ClosingDate = _announcement.ClosingDate,
                       Status = _announcement.Status,
                       LastUpdateDate = _announcement.LastUpdateDate,
                       AuthorId = _announcement.ShelterId,
                       PetId = _announcement.PetId,
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
        public async Task PostShouldAddAnnouncementWithPetID()
        {
            var request = new AnnouncementCreationRequest
            {
                Title = "test-annt2",
                Description = "test-de",
                PetId = _petId,
                ShelterId = _shelterId,
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements").PostJsonAsync(request);
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            var newAnnouncement = await response.GetJsonAsync<AnnouncementResponse>();
            newAnnouncement.Should().
                       BeEquivalentTo(new AnnouncementResponse
                       {
                           Id = Guid.NewGuid(),
                           Title = request.Title,
                           Description = request.Description,
                           CreationDate = DateTime.Now,
                           LastUpdateDate = DateTime.Now,
                           Status = 0,
                           PetId = (Guid)request.PetId,
                           AuthorId = request.ShelterId,
                       }, options => options.Excluding(s => s.Id).Excluding(s => s.CreationDate).Excluding(s => s.LastUpdateDate)); ;

            using var scope = _testSetup.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
            context.Announcements.Should().
                ContainEquivalentOf(new AnnouncementEntity
                {
                    Id = newAnnouncement.Id,
                    Title = newAnnouncement.Title,
                    Description = newAnnouncement.Description,
                    CreationDate = newAnnouncement.CreationDate,
                    ClosingDate = newAnnouncement.ClosingDate,
                    LastUpdateDate = newAnnouncement.LastUpdateDate,
                    Status = newAnnouncement.Status,
                    ShelterId = request.ShelterId,
                    PetId = (Guid)request.PetId,
                });
        }
        [Fact]
        public async Task PostShouldAddAnnouncementWithoutPetID()
        {
            var petRequest = new PetUpsertRequest
            {
                Name = "test-pet2",
                Breed = "test-breed2",
                Species = "test-species2",
                Birthday = DateTime.Now,
                Description = "test-description2",
                Photo = "test-photo2",
                ShelterId = _shelterId,
            };
            var request = new AnnouncementCreationRequest
            {
                Title = "test-annt2",
                Description = "test-de",
                PetRequest = petRequest,
                ShelterId = _shelterId,
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements").PostJsonAsync(request);
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            var newAnnouncement = await response.GetJsonAsync<AnnouncementResponse>();
            newAnnouncement.Should().
                       BeEquivalentTo(new AnnouncementResponse
                       {
                           Id = Guid.NewGuid(),
                           Title = request.Title,
                           Description = request.Description,
                           CreationDate = DateTime.Now,
                           LastUpdateDate = DateTime.Now,
                           Status = 0,
                           PetId = Guid.NewGuid(),
                           AuthorId = request.ShelterId,
                       }, options => options.Excluding(s => s.Id).Excluding(s => s.CreationDate).Excluding(s => s.LastUpdateDate).Excluding(p=>p.PetId)); ;
            
            using var scope = _testSetup.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
            context.Announcements.Should().
                ContainEquivalentOf(new AnnouncementEntity
                {
                    Id = newAnnouncement.Id,
                    Title = newAnnouncement.Title,
                    Description = newAnnouncement.Description,
                    CreationDate = newAnnouncement.CreationDate,
                    ClosingDate = newAnnouncement.ClosingDate,
                    LastUpdateDate = newAnnouncement.LastUpdateDate,
                    Status = newAnnouncement.Status,
                    ShelterId = request.ShelterId,
                    PetId = newAnnouncement.PetId,
                });
        }
        
        [Fact]
        public async Task PostShouldFailWithWrongShelterId()
        {
            Guid wrongShelterId = Guid.NewGuid();
            var request = new AnnouncementCreationRequest
            {
                Title = "test-announcement2",
                Description = "test-description2",
                PetId = _petId,
                ShelterId = wrongShelterId,
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements").PostJsonAsync(request);
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task PutShouldUpdateAnnouncement()
        {
            var request = new AnnouncementPutRequest
            {
                Status = 2,
                Description = "test-description-updated",
                ShelterId = _shelterId,
                PetId = _petId
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements", _announcement.Id).PutJsonAsync(request);
            response.StatusCode.Should().Be(200);
            var updatedAnnouncement = await response.GetJsonAsync<AnnouncementResponse>();
            updatedAnnouncement.Should().
                BeEquivalentTo(new AnnouncementResponse
                {
                    Id = _announcement.Id,
                    Title = _announcement.Title,
                    Description = request.Description,
                    Status = (int)request.Status,
                    AuthorId = (Guid)request.ShelterId,
                    PetId = (Guid)request.PetId,
                    CreationDate = _announcement.CreationDate,
                    LastUpdateDate = _announcement.LastUpdateDate,
                }, options=> options.Excluding(s=>s.LastUpdateDate));

            using var scope = _testSetup.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
            context.Announcements.Single(e => e.Id == _announcement.Id)
                .Should().BeEquivalentTo(new AnnouncementEntity
                {
                    Id = updatedAnnouncement.Id,
                    Title = updatedAnnouncement.Title,
                    Description = updatedAnnouncement.Description,
                    Status = updatedAnnouncement.Status,
                    ShelterId = updatedAnnouncement.AuthorId,
                    PetId = updatedAnnouncement.PetId,
                    CreationDate = updatedAnnouncement.CreationDate,
                    LastUpdateDate = updatedAnnouncement.LastUpdateDate
                });
        }

        [Fact]
        public async Task PutShouldFailWithWrongShelterId()
        {
            Guid wrongShelterId = Guid.NewGuid();
            var request = new AnnouncementPutRequest
            {
                Status = 2,
                Description = "test-description-updated",
                ShelterId = wrongShelterId,
                PetId = _petId,
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements", _announcement.Id).PutJsonAsync(request);
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task PutShouldFailWithWrongPetId()
        {
            Guid wrongPetId = Guid.NewGuid();
            var request = new AnnouncementPutRequest
            {
                Status = 2,
                Description = "test-description-updated",
                ShelterId = _shelterId,
                PetId = wrongPetId,
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements", _announcement.Id).PutJsonAsync(request);
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
        [Fact]
        public async Task PutShouldFailWithWrongAnnouncementId()
        {
            Guid wrongAnnouncementId = Guid.NewGuid();
            var request = new AnnouncementPutRequest
            {
                Status = 2,
                Description = "test-description-updated",
                ShelterId = _shelterId,
                PetId = _petId,
            };
            using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
            var response = await client.Request("announcements", wrongAnnouncementId).PutJsonAsync(request);
            response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
