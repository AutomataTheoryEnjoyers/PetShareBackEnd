using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShelterModule;
using ShelterModule.Controllers;
using ShelterModule.Models.Adopters;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Applications;
using Xunit;

namespace ShelterModuleTests;

[Trait("Category", "Integration")]
public sealed class ApplicationEndpointTests : IAsyncLifetime
{
    private readonly IReadOnlyList<AdopterEntity> _adopters;
    private readonly IReadOnlyList<AnnouncementEntity> _announcements;
    private readonly IReadOnlyList<ApplicationEntity> _applications;
    private readonly DateTime _now = new(2023, 4, 24);
    private readonly IReadOnlyList<PetEntity> _pets;
    private readonly IReadOnlyList<ShelterEntity> _shelters;
    private readonly IntegrationTestSetup _testSuite = new();

    public ApplicationEndpointTests()
    {
        _shelters = new ShelterEntity[]
        {
            new()
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
                Name = "pet1",
                Breed = "breed",
                Species = "species",
                Birthday = _now - TimeSpan.FromDays(60),
                Description = "description",
                Photo = "photo.jpg",
                ShelterId = _shelters[0].Id,
                Sex = PetSex.Unknown,
                Status = PetStatus.Active
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "pet2",
                Breed = "breed",
                Species = "species",
                Birthday = _now - TimeSpan.FromDays(128),
                Description = "description",
                Photo = "photo.png",
                ShelterId = _shelters[1].Id,
                Sex = PetSex.DoesNotApply,
                Status = PetStatus.Active
            }
        };
        _announcements = new AnnouncementEntity[]
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "announcement1",
                Description = "description",
                CreationDate = _now - TimeSpan.FromDays(10),
                Status = (int)AnnouncementStatus.Open,
                LastUpdateDate = _now - TimeSpan.FromDays(5),
                AuthorId = _shelters[0].Id,
                PetId = _pets[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "announcement2",
                Description = "description",
                CreationDate = _now - TimeSpan.FromDays(2),
                Status = (int)AnnouncementStatus.Open,
                LastUpdateDate = _now - TimeSpan.FromDays(1),
                AuthorId = _shelters[1].Id,
                PetId = _pets[1].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "announcement3",
                Description = "description",
                CreationDate = _now - TimeSpan.FromDays(4),
                Status = (int)AnnouncementStatus.Closed,
                LastUpdateDate = _now - TimeSpan.FromDays(3),
                AuthorId = _shelters[1].Id,
                PetId = _pets[1].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "announcement4",
                Description = "description",
                CreationDate = _now - TimeSpan.FromDays(25),
                Status = (int)AnnouncementStatus.DuringVerification,
                LastUpdateDate = _now - TimeSpan.FromDays(21),
                AuthorId = _shelters[0].Id,
                PetId = _pets[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "announcement5",
                Description = "description",
                CreationDate = _now - TimeSpan.FromDays(135),
                Status = (int)AnnouncementStatus.Open,
                LastUpdateDate = _now - TimeSpan.FromDays(1),
                AuthorId = _shelters[0].Id,
                PetId = _pets[0].Id
            }
        };
        _adopters = new AdopterEntity[]
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "adopter1",
                Email = "email1@mail.mail",
                PhoneNumber = "987654321",
                Status = AdopterStatus.Active,
                Address = new Address
                {
                    Country = "country",
                    Province = "province",
                    City = "city",
                    Street = "street",
                    PostalCode = "13-579"
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserName = "adopter2",
                Email = "email2@mail.mail",
                PhoneNumber = "987654321",
                Status = AdopterStatus.Active,
                Address = new Address
                {
                    Country = "country",
                    Province = "province",
                    City = "city",
                    Street = "street",
                    PostalCode = "09-909"
                }
            }
        };
        _applications = new ApplicationEntity[]
        {
            new()
            {
                Id = Guid.NewGuid(),
                CreationTime = _now - TimeSpan.FromHours(13),
                State = ApplicationState.Submitted,
                AdopterId = _adopters[0].Id,
                AnnouncementId = _announcements[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                CreationTime = _now - TimeSpan.FromHours(12),
                State = ApplicationState.Submitted,
                AdopterId = _adopters[1].Id,
                AnnouncementId = _announcements[1].Id
            }
        };
    }

    public async Task InitializeAsync()
    {
        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.AddRange(_shelters);
        context.Pets.AddRange(_pets);
        context.Announcements.AddRange(_announcements);
        context.Adopters.AddRange(_adopters);
        context.Applications.AddRange(_applications);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _testSuite.DisposeAsync();
    }

    [Fact]
    public async Task GetShouldReturnAllAppsForAdmin()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Admin);
        var apps = await client.Request("applications").GetJsonAsync<IEnumerable<ApplicationResponse>>();
        apps.Should().
             BeEquivalentTo(new ApplicationResponse[]
             {
                 new()
                 {
                     Id = _applications[0].Id,
                     CreationTime = _now - TimeSpan.FromHours(13),
                     State = ApplicationState.Submitted,
                     Adopter = new AdopterResponse
                     {
                         Id = _adopters[0].Id,
                         UserName = "adopter1",
                         Email = "email1@mail.mail",
                         PhoneNumber = "987654321",
                         Status = AdopterStatus.Active,
                         Address = _adopters[0].Address
                     },
                     AnnouncementId = _announcements[0].Id
                 },
                 new()
                 {
                     Id = _applications[1].Id,
                     CreationTime = _now - TimeSpan.FromHours(12),
                     State = ApplicationState.Submitted,
                     Adopter = new AdopterResponse
                     {
                         Id = _adopters[1].Id,
                         UserName = "adopter2",
                         Email = "email2@mail.mail",
                         PhoneNumber = "987654321",
                         Status = AdopterStatus.Active,
                         Address = _adopters[1].Address
                     },
                     AnnouncementId = _announcements[1].Id
                 }
             });
    }

    [Fact]
    public async Task GetShouldReturnAppsFromShelterForShelter()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id);
        var apps = await client.Request("applications").GetJsonAsync<IEnumerable<ApplicationResponse>>();
        apps.Should().
             BeEquivalentTo(new ApplicationResponse[]
             {
                 new()
                 {
                     Id = _applications[0].Id,
                     CreationTime = _now - TimeSpan.FromHours(13),
                     State = ApplicationState.Submitted,
                     Adopter = new AdopterResponse
                     {
                         Id = _adopters[0].Id,
                         UserName = "adopter1",
                         Email = "email1@mail.mail",
                         PhoneNumber = "987654321",
                         Status = AdopterStatus.Active,
                         Address = _adopters[0].Address
                     },
                     AnnouncementId = _announcements[0].Id
                 }
             });
    }

    [Fact]
    public async Task GetShouldReturnAppsByAdopterForAdopter()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Adopter, _adopters[0].Id);
        var apps = await client.Request("applications").GetJsonAsync<IEnumerable<ApplicationResponse>>();
        apps.Should().
             BeEquivalentTo(new ApplicationResponse[]
             {
                 new()
                 {
                     Id = _applications[0].Id,
                     CreationTime = _now - TimeSpan.FromHours(13),
                     State = ApplicationState.Submitted,
                     Adopter = new AdopterResponse
                     {
                         Id = _adopters[0].Id,
                         UserName = "adopter1",
                         Email = "email1@mail.mail",
                         PhoneNumber = "987654321",
                         Status = AdopterStatus.Active,
                         Address = _adopters[0].Address
                     },
                     AnnouncementId = _announcements[0].Id
                 }
             });
    }

    [Fact]
    public async Task PostShouldCreateNewApplication()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Adopter, _adopters[0].Id);
        var response = await client.Request("applications").
                                    PostJsonAsync(new ApplicationRequest
                                    {
                                        AnnouncementId = _announcements[4].Id
                                    });

        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        response.Headers.Should().ContainSingle(p => p.Name == "Location");

        var id = Guid.Parse(response.Headers.First(p => p.Name == "Location").Value);
        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Applications.Should().
                ContainSingle(app => app.Id == id).
                Which.Should().
                BeEquivalentTo(new ApplicationEntity
                {
                    Id = id,
                    AnnouncementId = _announcements[4].Id,
                    AdopterId = _adopters[0].Id,
                    CreationTime = DateTime.Now,
                    State = ApplicationState.Submitted
                },
                               options => options.Excluding(app => app.Adopter).
                                                  Excluding(app => app.Announcement).
                                                  Excluding(app => app.CreationTime));
    }

    [Fact]
    public async Task PostShouldNotCreateApplicationIfAnnouncementIsClosed()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Adopter, _adopters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications").
                                    PostJsonAsync(new ApplicationRequest
                                    {
                                        AnnouncementId = _announcements[2].Id
                                    });

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task PostShouldNotCreateApplicationIfAnnouncementIsDeleted()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Adopter, _adopters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications").
                                    PostJsonAsync(new ApplicationRequest
                                    {
                                        AnnouncementId = _announcements[2].Id
                                    });

        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  Id = _announcements[2].Id.ToString(),
                  ResourceName = nameof(Announcement)
              });
    }

    [Fact]
    public async Task PostShouldReturn404IfAnnouncementDoesntExist()
    {
        var invalidId = Guid.NewGuid();
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Adopter, _adopters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications").
                                    PostJsonAsync(new ApplicationRequest
                                    {
                                        AnnouncementId = invalidId
                                    });

        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  Id = invalidId.ToString(),
                  ResourceName = nameof(Announcement)
              });
    }

    [Fact]
    public async Task GetShouldReturnApplicationById()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id);
        var app = await client.Request("applications", _applications[0].Id).GetJsonAsync<ApplicationResponse>();
        app.Should().
            BeEquivalentTo(new ApplicationResponse
            {
                Id = _applications[0].Id,
                CreationTime = _now - TimeSpan.FromHours(13),
                State = ApplicationState.Submitted,
                Adopter = new AdopterResponse
                {
                    Id = _adopters[0].Id,
                    UserName = "adopter1",
                    Email = "email1@mail.mail",
                    PhoneNumber = "987654321",
                    Status = AdopterStatus.Active,
                    Address = _adopters[0].Address
                },
                AnnouncementId = _announcements[0].Id
            });
    }

    [Fact]
    public async Task GetShouldReturn404IfApplicationDoesntExist()
    {
        var invalidId = Guid.NewGuid();
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications", invalidId).GetAsync();

        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var error = await response.GetJsonAsync<NotFoundResponse>();
        error.Should().
              BeEquivalentTo(new NotFoundResponse
              {
                  Id = invalidId.ToString(),
                  ResourceName = nameof(Application)
              });
    }

    [Fact]
    public async Task GetShouldReturn403IfApplicationBelongsToDifferentShelter()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications", _applications[1].Id).GetAsync();

        response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task WithdrawShouldWithdrawApplication()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Adopter, _adopters[0].Id);
        var response = await client.Request("applications", _applications[0].Id, "withdraw").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Withdrawn);
    }

    [Fact]
    public async Task WithdrawShouldReturn403IfAppBelongsToDifferentAdopter()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Adopter, _adopters[1].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications", _applications[0].Id, "withdraw").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Submitted);
    }

    [Fact]
    public async Task AcceptShouldAcceptApplication()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id);
        var response = await client.Request("applications", _applications[0].Id, "accept").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Accepted);
    }

    [Fact]
    public async Task AcceptShouldReturn403IfAppBelongsToDifferentShelter()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[1].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications", _applications[0].Id, "accept").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Submitted);
    }

    [Fact]
    public async Task AcceptShouldReturn400IfApplicationIsWithdrawn()
    {
        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        await context.Applications.Where(app => app.Id == _applications[0].Id).
                      ExecuteUpdateAsync(app => app.SetProperty(e => e.State, ApplicationState.Withdrawn));

        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications", _applications[0].Id, "accept").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Withdrawn);
    }

    [Fact]
    public async Task AcceptShouldReturn400IfApplicationIsRejected()
    {
        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        await context.Applications.Where(app => app.Id == _applications[0].Id).
                      ExecuteUpdateAsync(app => app.SetProperty(e => e.State, ApplicationState.Rejected));

        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications", _applications[0].Id, "accept").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Rejected);
    }

    [Fact]
    public async Task RejectShouldRejectApplication()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id);
        var response = await client.Request("applications", _applications[0].Id, "reject").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status200OK);

        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Rejected);
    }

    [Fact]
    public async Task RejectShouldReturn400IfApplicationIsAccepted()
    {
        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        await context.Applications.Where(app => app.Id == _applications[0].Id).
                      ExecuteUpdateAsync(app => app.SetProperty(e => e.State, ApplicationState.Accepted));

        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[0].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications", _applications[0].Id, "reject").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Accepted);
    }

    [Fact]
    public async Task RejectShouldReturn403IfAppBelongsToDifferentShelter()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Shelter, _shelters[1].Id).AllowAnyHttpStatus();
        var response = await client.Request("applications", _applications[0].Id, "reject").PutAsync();

        response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Applications.Single(app => app.Id == _applications[0].Id).State.Should().Be(ApplicationState.Submitted);
    }
}
