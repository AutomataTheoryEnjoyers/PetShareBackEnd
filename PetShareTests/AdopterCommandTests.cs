using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using PetShare.Services.Implementations.Adopters;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Unit")]
public sealed class AdopterCommandTests : IAsyncLifetime
{
    private readonly AdopterEntity _adopter = new()
    {
        Id = Guid.NewGuid(),
        UserName = "adopter-1",
        Email = "adopter@mail.com",
        PhoneNumber = "123456789",
        Status = AdopterStatus.Deleted,
        DeletionTime = new DateTime(2023, 3, 1, 1, 1, 1),
        Address = new Address
        {
            Country = "country",
            Province = "province",
            City = "city",
            Street = "street",
            PostalCode = "12-345"
        }
    };

    private readonly AdopterCommand _command;
    private readonly TestDbConnectionString _connection = IntegrationTestSetup.CreateTestDatabase();

    public AdopterCommandTests()
    {
        _command = new AdopterCommand(IntegrationTestSetup.CreateDbContext(_connection));
    }

    public async Task InitializeAsync()
    {
        var context = IntegrationTestSetup.CreateDbContext(_connection);
        context.Adopters.Add(_adopter);
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _connection.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldRemoveDeletedAdopter()
    {
        await _command.RemoveDeletedAsync(_adopter.DeletionTime!.Value + TimeSpan.FromDays(1));
        var context = IntegrationTestSetup.CreateDbContext(_connection);
        context.Adopters.Should().BeEmpty();
    }
}
