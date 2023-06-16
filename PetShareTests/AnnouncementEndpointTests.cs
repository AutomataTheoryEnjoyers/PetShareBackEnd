using Azure.Core;
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
    private readonly AdopterEntity _adopter;
    private readonly AnnouncementEntity[] _announcements;
    private readonly PetEntity[] _pets;
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
                ShelterId = _shelter.Id,
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
                ShelterId = _shelter.Id,
                Sex = PetSex.Unknown,
                Status = PetStatus.Active
            }
        };

        _announcements = new AnnouncementEntity[]
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "test-announcement1",
                Description = "test-description",
                CreationDate = DateTime.Now,
                Status = 0,
                LastUpdateDate = DateTime.Now,
                AuthorId = _shelter.Id,
                PetId = _pets[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "test-announcement2",
                Description = "test-description",
                CreationDate = DateTime.Now,
                Status = 0,
                LastUpdateDate = DateTime.Now,
                AuthorId = _shelter.Id,
                PetId = _pets[1].Id
            }
        };

        _adopter = new AdopterEntity
        {
            Id = Guid.NewGuid(),
            UserName = "adopter-1",
            Email = "mail@mail.com",
            PhoneNumber = "719302889",
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
    }

    public async Task InitializeAsync()
    {
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Add(_shelter);
        context.Pets.AddRange(_pets);
        context.Announcements.AddRange(_announcements);
        context.Adopters.Add(_adopter);
        context.Likes.Add(new LikedAnnouncementEntity
        {
            AnnouncementId = _announcements[0].Id,
            AdopterId = _adopter.Id
        });
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
        var announcements = await response.GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        announcements.Should().
                      BeEquivalentTo(new PaginatedLikedAnnouncementsResponse
                      {
                          Announcements = new[]
                          {
                              new LikedAnnouncementResponse
                              {
                                  Id = _announcements[0].Id,
                                  Title = _announcements[0].Title,
                                  Description = _announcements[0].Description,
                                  CreationDate = _announcements[0].CreationDate,
                                  ClosingDate = _announcements[0].ClosingDate,
                                  Status = _announcements[0].Status.ToString().ToLower(),
                                  LastUpdateDate = _announcements[0].LastUpdateDate,
                                  Pet = Pet.FromEntity(_announcements[0].Pet).ToResponse(),
                                  IsLiked = false
                              },
                              new LikedAnnouncementResponse
                              {
                                  Id = _announcements[1].Id,
                                  Title = _announcements[1].Title,
                                  Description = _announcements[1].Description,
                                  CreationDate = _announcements[1].CreationDate,
                                  ClosingDate = _announcements[1].ClosingDate,
                                  Status = _announcements[1].Status.ToString().ToLower(),
                                  LastUpdateDate = _announcements[1].LastUpdateDate,
                                  Pet = Pet.FromEntity(_announcements[1].Pet).ToResponse(),
                                  IsLiked = false
                              }
                          },
                          PageNumber = 0,
                          Count = 2
                      });
    }

    [Fact]
    public async Task GetShouldFetchAnnouncementsFromShelter()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Shelter, _shelter.Id).AllowAnyHttpStatus();
        var response = await client.Request("shelter", "announcements").GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<PaginatedAnnouncementsResponse>();
        announcements.Should().
                      BeEquivalentTo(new PaginatedAnnouncementsResponse
                      {
                          Announcements = _announcements.Select(a => Announcement.FromEntity(a).ToResponse()).ToList(),
                          PageNumber = 0,
                          Count = 2
                      });
    }

    [Fact]
    public async Task GetAnnouncementsWithFiltersEmpty()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Adopter, _adopter.Id).AllowAnyHttpStatus();
        var query = new GetAllAnnouncementsFilteredQueryRequest
        {
            MinAge = 1000
        };
        var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var pets = await response.GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        pets.Announcements.Should().BeEmpty();
        pets.Count.Should().Be(0);
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
        var announcements = await response.GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        announcements.Should().
                      BeEquivalentTo(new PaginatedLikedAnnouncementsResponse
                      {
                          Announcements = new[]
                          {
                              new LikedAnnouncementResponse
                              {
                                  Id = _announcements[0].Id,
                                  Title = _announcements[0].Title,
                                  Description = _announcements[0].Description,
                                  CreationDate = _announcements[0].CreationDate,
                                  ClosingDate = _announcements[0].ClosingDate,
                                  Status = _announcements[0].Status.ToString().ToLower(),
                                  LastUpdateDate = _announcements[0].LastUpdateDate,
                                  Pet = Pet.FromEntity(_announcements[0].Pet).ToResponse(),
                                  IsLiked = false
                              },
                              new LikedAnnouncementResponse
                              {
                                  Id = _announcements[1].Id,
                                  Title = _announcements[1].Title,
                                  Description = _announcements[1].Description,
                                  CreationDate = _announcements[1].CreationDate,
                                  ClosingDate = _announcements[1].ClosingDate,
                                  Status = _announcements[1].Status.ToString().ToLower(),
                                  LastUpdateDate = _announcements[1].LastUpdateDate,
                                  Pet = Pet.FromEntity(_announcements[1].Pet).ToResponse(),
                                  IsLiked = false
                              }
                          },
                          PageNumber = 0,
                          Count = 2
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
        var announcements = await response.GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        announcements.Should().
                      BeEquivalentTo(new PaginatedLikedAnnouncementsResponse
                      {
                          Announcements = new[]
                          {
                              new LikedAnnouncementResponse
                              {
                                  Id = _announcements[0].Id,
                                  Title = _announcements[0].Title,
                                  Description = _announcements[0].Description,
                                  CreationDate = _announcements[0].CreationDate,
                                  ClosingDate = _announcements[0].ClosingDate,
                                  Status = _announcements[0].Status.ToString().ToLower(),
                                  LastUpdateDate = _announcements[0].LastUpdateDate,
                                  Pet = Pet.FromEntity(_announcements[0].Pet).ToResponse(),
                                  IsLiked = false
                              },
                              new LikedAnnouncementResponse
                              {
                                  Id = _announcements[1].Id,
                                  Title = _announcements[1].Title,
                                  Description = _announcements[1].Description,
                                  CreationDate = _announcements[1].CreationDate,
                                  ClosingDate = _announcements[1].ClosingDate,
                                  Status = _announcements[1].Status.ToString().ToLower(),
                                  LastUpdateDate = _announcements[1].LastUpdateDate,
                                  Pet = Pet.FromEntity(_announcements[1].Pet).ToResponse(),
                                  IsLiked = false
                              }
                          },
                          PageNumber = 0,
                          Count = 2
                      });
    }

    [Fact]
    public async Task GetPaginatedAnnouncementsWithFiltersNonEmpty()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new GetAllAnnouncementsFilteredQueryRequest
        {
            PageNumber = 0,
            PageCount = 1,
            MaxAge = 1000,
            Status = "open"
        };
        var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        announcements.Announcements.Count.Should().Be(1);
        announcements.PageNumber.Should().Be(0);
        announcements.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetPaginatedAnnouncementsWithFiltersEmpty()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new GetAllAnnouncementsFilteredQueryRequest
        {
            PageNumber = 0,
            PageCount = 1
        };
        var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        announcements.Announcements.Count.Should().Be(1);
        announcements.PageNumber.Should().Be(0);
        announcements.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetShouldFailWithWrongPaginationParams()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var query = new GetAllAnnouncementsFilteredQueryRequest
        {
            PageCount = -10,
            PageNumber = -10
        };
        var response = await client.Request("announcements").SetQueryParams(query).GetAsync();
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GetShouldFetchAnnouncementById()
    {
        using var client = _testSetup.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("announcements", _announcements[0].Id).GetAsync();
        response.StatusCode.Should().Be(200);
        var announcements = await response.GetJsonAsync<AnnouncementResponse>();
        announcements.Should().
                      BeEquivalentTo(new AnnouncementResponse
                      {
                          Id = _announcements[0].Id,
                          Title = _announcements[0].Title,
                          Description = _announcements[0].Description,
                          CreationDate = _announcements[0].CreationDate,
                          ClosingDate = _announcements[0].ClosingDate,
                          Status = _announcements[0].Status.ToString().ToLower(),
                          LastUpdateDate = _announcements[0].LastUpdateDate,
                          Pet = Pet.FromEntity(_announcements[0].Pet).ToResponse()
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
            PetId = _pets[0].Id
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
                            Status = AnnouncementStatus.Open.ToString().ToLower(),
                            Pet = Pet.FromEntity(_pets[0]).ToResponse()
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
                    Status = Enum.Parse<AnnouncementStatus>(newAnnouncement.Status,true),
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
        var response = await client.Request("announcements", _announcements[0].Id).PutJsonAsync(request);
        response.StatusCode.Should().Be(200);
        var updatedAnnouncement = await response.GetJsonAsync<AnnouncementResponse>();
        updatedAnnouncement.Should().
                            BeEquivalentTo(new AnnouncementResponse
                            {
                                Id = _announcements[0].Id,
                                Title = _announcements[0].Title,
                                Description = request.Description,
                                Status = request.Status.ToLower(),
                                Pet = Pet.FromEntity(_announcements[0].Pet).ToResponse(),
                                CreationDate = _announcements[0].CreationDate,
                                ClosingDate = null,
                                LastUpdateDate = _announcements[0].LastUpdateDate
                            }, options => options.Excluding(s => s.LastUpdateDate));

        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Announcements.Single(e => e.Id == _announcements[0].Id).
                Should().
                BeEquivalentTo(new AnnouncementEntity
                {
                    Id = updatedAnnouncement.Id,
                    Title = updatedAnnouncement.Title,
                    Description = updatedAnnouncement.Description,
                    Status = Enum.Parse<AnnouncementStatus>(updatedAnnouncement.Status,true),
                    PetId = updatedAnnouncement.Pet.Id,
                    CreationDate = updatedAnnouncement.CreationDate,
                    LastUpdateDate = updatedAnnouncement.LastUpdateDate,
                    AuthorId = _shelter.Id
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

    [Fact]
    public async Task PutShouldLikeTheAnnouncement()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Adopter, _adopter.Id);

        var response =
           await client.Request("announcements").GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        response.Announcements.Should().
                 ContainEquivalentOf(new LikedAnnouncementResponse
                 {
                     Id = _announcements[1].Id,
                     Title = _announcements[1].Title,
                     Description = _announcements[1].Description,
                     CreationDate = _announcements[1].CreationDate,
                     ClosingDate = _announcements[1].ClosingDate,
                     Status = _announcements[1].Status.ToString(),
                     LastUpdateDate = _announcements[1].LastUpdateDate,
                     Pet = Pet.FromEntity(_announcements[1].Pet).ToResponse(),
                     IsLiked = false
                 });

        var likeResponse = await client.Request($"announcements/{_announcements[1].Id}/like").PutAsync();

        likeResponse.StatusCode.Should().Be(StatusCodes.Status200OK);

        var getLikedResponse =
           await client.Request("announcements").GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        getLikedResponse.Announcements.Should().
                 ContainEquivalentOf(new LikedAnnouncementResponse
                 {
                     Id = _announcements[1].Id,
                     Title = _announcements[1].Title,
                     Description = _announcements[1].Description,
                     CreationDate = _announcements[1].CreationDate,
                     ClosingDate = _announcements[1].ClosingDate,
                     Status = _announcements[1].Status.ToString(),
                     LastUpdateDate = _announcements[1].LastUpdateDate,
                     Pet = Pet.FromEntity(_announcements[1].Pet).ToResponse(),
                     IsLiked = true
                 });
       
        using var scope = _testSetup.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Likes.Should().
                ContainEquivalentOf(new LikedAnnouncementEntity
                {
                    AdopterId = _adopter.Id,
                    AnnouncementId = _announcements[1].Id
                });
    }

    [Fact]
    public async Task GetShouldIncludeInfoAboutLikes()
    {
        using var client = _testSetup.CreateFlurlClient().WithAuth(Roles.Adopter, _adopter.Id);
        var response =
            await client.Request("announcements").GetJsonAsync<PaginatedLikedAnnouncementsResponse>();
        response.Announcements.Should().
                 ContainEquivalentOf(new LikedAnnouncementResponse
                 {
                     Id = _announcements[0].Id,
                     Title = _announcements[0].Title,
                     Description = _announcements[0].Description,
                     CreationDate = _announcements[0].CreationDate,
                     ClosingDate = _announcements[0].ClosingDate,
                     Status = _announcements[0].Status.ToString().ToLower(),
                     LastUpdateDate = _announcements[0].LastUpdateDate,
                     Pet = Pet.FromEntity(_announcements[0].Pet).ToResponse(),
                     IsLiked = true
                 });
    }
}
