﻿using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace PetShareTests;

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
            PhoneNumber = "123456789",
            Address = new Address
            {
                Country = "test-country",
                Province = "test-province",
                City = "test-city",
                Street = "test-street",
                PostalCode = "test-postalCode"
            }
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
        (await client.Request("shelter").GetAsync()).StatusCode.Should().Be(200);
    }
}
