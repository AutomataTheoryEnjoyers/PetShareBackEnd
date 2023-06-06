using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetShare.Controllers;
using PetShare.Models.Reports;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Integration")]
public sealed class ReportEndpointTests : IAsyncLifetime
{
    private readonly IReadOnlyList<ReportEntity> _reports;
    private readonly ShelterEntity _shelter;
    private readonly IntegrationTestSetup _testSuite = new();

    public ReportEndpointTests()
    {
        var now = new DateTime(2023, 6, 6, 17, 24, 0);
        _shelter = new ShelterEntity
        {
            Id = Guid.NewGuid(),
            UserName = "shelter-1",
            FullShelterName = "Shelter 1",
            Email = "shelter@one.com",
            PhoneNumber = "239045312",
            IsAuthorized = true,
            Address = new Address
            {
                Country = "country",
                Province = "province",
                City = "city",
                PostalCode = "123-45",
                Street = "street"
            }
        };

        _reports = new[]
        {
            new ReportEntity
            {
                Id = Guid.NewGuid(),
                TargetId = _shelter.Id,
                TargetType = ReportedEntityType.Shelter,
                State = ReportState.New,
                Message = "message 1",
                CreationTime = now - TimeSpan.FromSeconds(10)
            },
            new ReportEntity
            {
                Id = Guid.NewGuid(),
                TargetId = _shelter.Id,
                TargetType = ReportedEntityType.Shelter,
                State = ReportState.New,
                Message = "message 2",
                CreationTime = now - TimeSpan.FromSeconds(20)
            },
            new ReportEntity
            {
                Id = Guid.NewGuid(),
                TargetId = _shelter.Id,
                TargetType = ReportedEntityType.Shelter,
                State = ReportState.New,
                Message = "message 3",
                CreationTime = now - TimeSpan.FromSeconds(30)
            },
            new ReportEntity
            {
                Id = Guid.NewGuid(),
                TargetId = _shelter.Id,
                TargetType = ReportedEntityType.Shelter,
                State = ReportState.Accepted,
                Message = "message 4",
                CreationTime = now - TimeSpan.FromSeconds(40)
            }
        };
    }

    public async Task InitializeAsync()
    {
        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Shelters.Add(_shelter);
        context.Reports.AddRange(_reports);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _testSuite.DisposeAsync();
    }

    [Fact]
    public async Task GetShouldReturnReportsWithoutQueryParams()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Admin);
        var response = await client.Request("reports").GetJsonAsync<ReportPageResponse>();

        response.Should().
                 BeEquivalentTo(new ReportPageResponse
                 {
                     Reports = new[]
                     {
                         new ReportResponse
                         {
                             Id = _reports[0].Id,
                             TargetId = _reports[0].TargetId,
                             ReportType = "shelter",
                             State = "new",
                             Message = _reports[0].Message
                         },
                         new ReportResponse
                         {
                             Id = _reports[1].Id,
                             TargetId = _reports[1].TargetId,
                             ReportType = "shelter",
                             State = "new",
                             Message = _reports[1].Message
                         },
                         new ReportResponse
                         {
                             Id = _reports[2].Id,
                             TargetId = _reports[2].TargetId,
                             ReportType = "shelter",
                             State = "new",
                             Message = _reports[2].Message
                         }
                     },
                     PageNumber = 0,
                     Count = 3
                 });
    }

    [Fact]
    public async Task GetShouldCorrectlyUseQueryParams()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Admin);
        var response = await client.Request("reports").
                                    SetQueryParams(new { pageNumber = 1, pageCount = 2 }).
                                    GetJsonAsync<ReportPageResponse>();

        response.Should().
                 BeEquivalentTo(new ReportPageResponse
                 {
                     Reports = new[]
                     {
                         new ReportResponse
                         {
                             Id = _reports[2].Id,
                             TargetId = _reports[2].TargetId,
                             ReportType = "shelter",
                             State = "new",
                             Message = _reports[2].Message
                         }
                     },
                     PageNumber = 1,
                     Count = 3
                 });
    }

    [Fact]
    public async Task ShouldValidatePageNumber()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Admin).AllowAnyHttpStatus();
        var response = await client.Request("reports").
                                    SetQueryParams(new { pageNumber = -1, pageCount = 2 }).
                                    GetAsync();

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ShouldValidatePageCount()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Admin).AllowAnyHttpStatus();
        var response = await client.Request("reports").
                                    SetQueryParams(new { pageNumber = 1, pageCount = 0 }).
                                    GetAsync();

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task PostShouldCreateNewReport()
    {
        var request = new ReportRequest
        {
            TargetId = _shelter.Id,
            ReportType = "shelter",
            Message = "new message"
        };

        using var client = _testSuite.CreateFlurlClient();
        var response = await client.Request("reports").PostJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        response.Headers.Should().ContainSingle(p => p.Name == "Location");

        var id = Guid.Parse(response.Headers.First(p => p.Name == "Location").Value);
        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();

        var entity = await context.Reports.SingleAsync(report => report.Id == id);
        entity.Should().
               BeEquivalentTo(new ReportEntity
               {
                   Id = id,
                   TargetId = request.TargetId,
                   TargetType = ReportedEntityType.Shelter,
                   State = ReportState.New,
                   Message = "new message",
                   CreationTime = DateTime.Now
               }, options => options.Excluding(report => report.CreationTime));
    }

    [Fact]
    public async Task PostShouldValidateTargetType()
    {
        var request = new ReportRequest
        {
            TargetId = _shelter.Id,
            ReportType = "alien",
            Message = "new message"
        };

        using var client = _testSuite.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("reports").PostJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task PostShouldValidateTargetId()
    {
        var request = new ReportRequest
        {
            TargetId = _shelter.Id,
            ReportType = "adopter",
            Message = "new message"
        };

        using var client = _testSuite.CreateFlurlClient().AllowAnyHttpStatus();
        var response = await client.Request("reports").PostJsonAsync(request);
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Reports.Where(report => report.Message == request.Message).Should().BeEmpty();
    }

    [Fact]
    public async Task PutShouldUpdateStatus()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Admin);
        var response = await client.Request("reports", _reports[0].Id).PutJsonAsync(new { state = "accepted" });
        response.StatusCode.Should().Be(StatusCodes.Status200OK);

        var report = await response.GetJsonAsync<ReportResponse>();
        report.Should().
               BeEquivalentTo(new ReportResponse
               {
                   Id = _reports[0].Id,
                   TargetId = _reports[0].TargetId,
                   ReportType = "shelter",
                   State = "accepted",
                   Message = _reports[0].Message
               });

        using var scope = _testSuite.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        var reportEntity = await context.Reports.FirstAsync(entity => entity.Id == _reports[0].Id);
        reportEntity.State.Should().Be(ReportState.Accepted);
    }

    [Fact]
    public async Task PutShouldValidateStatus()
    {
        using var client = _testSuite.CreateFlurlClient().WithAuth(Roles.Admin).AllowAnyHttpStatus();
        var response = await client.Request("reports", _reports[0].Id).PutJsonAsync(new { state = "updated" });
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
