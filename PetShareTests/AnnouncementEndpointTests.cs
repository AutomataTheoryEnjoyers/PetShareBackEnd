using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PetShare.Controllers;
using PetShare.Models.Announcements;
using PetShare.Models.Pets;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Integration")]
public sealed class AnnouncementEndpointTests : IAsyncLifetime
{
    private readonly AnnouncementEntity _announcement;
    private readonly PetEntity _pet;
    private readonly ShelterEntity _shelter;

    private readonly IntegrationTestSetup _testSetup = new();

    public AnnouncementEndpointTests()
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
            ShelterId = _shelter.Id,
            Sex = PetSex.Unknown,
            Status = PetStatus.Active
        };

        _announcement = new AnnouncementEntity
        {
            Id = Guid.NewGuid(),
            Title = "test-announcement",
            Description = "test-description",
            CreationDate = DateTime.Now,
            Status = 0,
            LastUpdateDate = DateTime.Now,
            AuthorId = _shelter.Id,
            PetId = _pet.Id
        };
    }

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
        await _testSetup.DisposeAsync();
    }

    [Fact]
    public async Task GetShouldFetchAllAnnouncements()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("announcements").GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<IEnumerable<AnnouncementResponse>>();
        announcements.Should().
                      BeEquivalentTo(new[]
                      {
                          new AnnouncementResponse
                          {
                              Id = _announcement.Id,
                              Title = _announcement.Title,
                              Description = _announcement.Description,
                              CreationDate = _announcement.CreationDate,
                              ClosingDate = _announcement.ClosingDate,
                              Status = _announcement.Status.ToString(),
                              LastUpdateDate = _announcement.LastUpdateDate,
                              Pet = Pet.FromEntity(_announcement.Pet).ToResponse()
                          }
                      });
    }

    [Fact]
    public async Task GetShouldFetchAnnouncementsFromShelter()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
        var response = await client.Request("shelter", "announcements").GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<IEnumerable<AnnouncementResponse>>();
        announcements.Should().
                      BeEquivalentTo(new[]
                      {
                          new AnnouncementResponse
                          {
                              Id = _announcement.Id,
                              Title = _announcement.Title,
                              Description = _announcement.Description,
                              CreationDate = _announcement.CreationDate,
                              ClosingDate = _announcement.ClosingDate,
                              Status = _announcement.Status.ToString(),
                              LastUpdateDate = _announcement.LastUpdateDate,
                              Pet = Pet.FromEntity(_announcement.Pet).ToResponse()
                          }
                      });
    }

    [Fact]
    public async Task GetAnnouncementsWithFiltersEmpty()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new GetAllAnnouncementsFilteredQueryRequest
        {
            MinAge = 1000
        };
        var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var pets = await response.GetJsonAsync<IEnumerable<AnnouncementResponse>>();
        pets.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAnnouncementsWithAdvancedFilters()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new GetAllAnnouncementsFilteredQueryRequest
        {
            Breeds = new List<string> { "test-breed" },
            Species = new List<string> { "test-species" }
        };
        var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<IEnumerable<AnnouncementResponse>>();
        announcements.Should().
                      BeEquivalentTo(new[]
                      {
                          new AnnouncementResponse
                          {
                              Id = _announcement.Id,
                              Title = _announcement.Title,
                              Description = _announcement.Description,
                              CreationDate = _announcement.CreationDate,
                              ClosingDate = _announcement.ClosingDate,
                              Status = _announcement.Status.ToString(),
                              LastUpdateDate = _announcement.LastUpdateDate,
                              Pet = Pet.FromEntity(_announcement.Pet).ToResponse()
                          }
                      });
    }

    [Fact]
    public async Task GetAnnouncementsWithFiltersNonEmpty()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new GetAllAnnouncementsFilteredQueryRequest
        {
            MaxAge = 1000
        };
        var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<IEnumerable<AnnouncementResponse>>();
        announcements.Should().
                      BeEquivalentTo(new[]
                      {
                          new AnnouncementResponse
                          {
                              Id = _announcement.Id,
                              Title = _announcement.Title,
                              Description = _announcement.Description,
                              CreationDate = _announcement.CreationDate,
                              ClosingDate = _announcement.ClosingDate,
                              Status = _announcement.Status.ToString(),
                              LastUpdateDate = _announcement.LastUpdateDate,
                              Pet = Pet.FromEntity(_announcement.Pet).ToResponse()
                          }
                      });
    }

    [Fact]
    public async Task GetShouldFetchAnnouncementById()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("announcements", _announcement.Id).GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<AnnouncementResponse>();
        announcements.Should().
                      BeEquivalentTo(new AnnouncementResponse
                      {
                          Id = _announcement.Id,
                          Title = _announcement.Title,
                          Description = _announcement.Description,
                          CreationDate = _announcement.CreationDate,
                          ClosingDate = _announcement.ClosingDate,
                          Status = _announcement.Status.ToString(),
                          LastUpdateDate = _announcement.LastUpdateDate,
                          Pet = Pet.FromEntity(_announcement.Pet).ToResponse()
                      });
    }

    [Fact]
    public async Task GetShouldFailWithWrongPetId()
    {
        var wrongId = Guid.NewGuid();
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("pet", wrongId).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task PostShouldAddAnnouncementWithPetId()
    {
        var request = new AnnouncementCreationRequest
        {
            Title = "test-annt2",
            Description = "test-de",
            PetId = _pet.Id
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
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
                            ClosingDate = null,
                            LastUpdateDate = DateTime.Now,
                            Status = AnnouncementStatus.Open.ToString(),
                            Pet = Pet.FromEntity(_pet).ToResponse(),
                        },
                                       options => options.Excluding(s => s.Id).
                                                          Excluding(s => s.CreationDate).
                                                          Excluding(s => s.LastUpdateDate));
        ;

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
                    Status = Enum.Parse<AnnouncementStatus>(newAnnouncement.Status),
                    AuthorId = _shelter.Id,
                    PetId = request.PetId
                });
    }

    [Fact]
    public async Task PutShouldUpdateAnnouncement()
    {
        var request = new AnnouncementPutRequest
        {
            Status = AnnouncementStatus.Open.ToString(),
            Description = "test-description-updated"
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
        var response = await client.Request("announcements", _announcement.Id).PutJsonAsync(request);
        response.StatusCode.Should().Be(200);
        var updatedAnnouncement = await response.GetJsonAsync<AnnouncementResponse>();
        updatedAnnouncement.Should().
                            BeEquivalentTo(new AnnouncementResponse
                            {
                                Id = _announcement.Id,
                                Title = _announcement.Title,
                                Description = request.Description,
                                Status = request.Status.ToString(),
                                Pet = Pet.FromEntity(_announcement.Pet).ToResponse(),
                                CreationDate = _announcement.CreationDate,
                                ClosingDate = null,
                                LastUpdateDate = _announcement.LastUpdateDate
                            }, options => options.Excluding(s => s.LastUpdateDate));

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Announcements.Single(e => e.Id == _announcement.Id).
                Should().
                BeEquivalentTo(new AnnouncementEntity
                {
                    Id = updatedAnnouncement.Id,
                    Title = updatedAnnouncement.Title,
                    Description = updatedAnnouncement.Description,
                    Status = Enum.Parse<AnnouncementStatus>(updatedAnnouncement.Status),
                    PetId = updatedAnnouncement.Pet.Id,
                    CreationDate = updatedAnnouncement.CreationDate,
                    LastUpdateDate = updatedAnnouncement.LastUpdateDate,
                    AuthorId = _shelter.Id,
                });
    }

    [Fact]
    public async Task PutShouldFailWithWrongAnnouncementId()
    {
        var wrongAnnouncementId = Guid.NewGuid();
        var request = new AnnouncementPutRequest
        {
            Status = AnnouncementStatus.Open.ToString(),
            Description = "test-description-updated"
        };
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
        var response = await client.Request("announcements", wrongAnnouncementId).PutJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
