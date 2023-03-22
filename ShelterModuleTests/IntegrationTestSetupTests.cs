using Database;
using Database.Entities;
using FluentAssertions;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ShelterModuleTests;

[Trait("Category", "Unit")]
public sealed class IntegrationTestSetupTests
{
    [Theory]
    [InlineData("first value")]
    [InlineData("second value")]
    public async Task ShouldUseUniqueDbPerTest(string shelterName)
    {
        using var connection = IntegrationTestSetup.CreateTestDatabase();
        await using var context = IntegrationTestSetup.CreateDbContext(connection);

        context.Shelters.Should().BeEmpty();

        context.Shelters.Add(new ShelterEntity
        {
            Id = Guid.NewGuid(),
            IsAuthorized = false,
            UserName = shelterName,
            FullShelterName = shelterName,
            Email = "mail@mail.mail",
            PhoneNumber = "123456789"
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public void ShouldDeleteDbAfterTest()
    {
        string connectionString;
        using (var connection = IntegrationTestSetup.CreateTestDatabase())
        {
            connectionString = connection.ConnectionString;
        }

        using var context =
            new PetShareDbContext(new DbContextOptionsBuilder<PetShareDbContext>().UseSqlServer(connectionString).
                                                                                   Options);
        context.Database.EnsureDeleted().Should().BeFalse();
    }

    [Fact]
    public async Task ShouldCreateWorkingFlurlClient()
    {
        var client = new IntegrationTestSetup().CreateFlurlClient();
        (await client.Request("shelters").GetAsync()).StatusCode.Should().Be(200);
    }
}
