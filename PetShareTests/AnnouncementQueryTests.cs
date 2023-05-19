using Database;
using Database.Entities;
using Database.ValueObjects;
using PetShare.Models.Announcements;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Unit")]
public sealed class AnnouncementQueryTests : IAsyncLifetime
{
    private readonly TestDbConnectionString _connection;
    private readonly PetShareDbContext _context;
    private readonly IReadOnlyList<ShelterEntity> _shelters;

    public AnnouncementQueryTests()
    {
        _connection = IntegrationTestSetup.CreateTestDatabase();
        _context = IntegrationTestSetup.CreateDbContext(_connection);
        
        _shelters = new[]
        {
            new ShelterEntity
            {
                Id = Guid.NewGuid(),
                UserName = "shelter1",
                FullShelterName = "Shelter 1",
                Email = "shelter1@mail.com",
                PhoneNumber = "135790246",
                IsAuthorized = true,
                Address = new Address
                {
                    Country = "Iran",
                    City = "Baku",
                    PostalCode = "69-420",
                    Province = "West Virginia",
                    Street = "Wall"
                }
            }
        }
    }

    public async Task InitializeAsync()
    {
        var announcement = new AnnouncementEntity
        {
            Id = Guid.NewGuid(),
            CreationDate = new DateTime(2023, 05, 16),
            LastUpdateDate = new DateTime(2023, 05, 19),
            ClosingDate = null,
            AuthorId = Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            Title = "Announcement 1",
            Description = "Test announcement",
            Status = (int)AnnouncementStatus.Open
        };
    }

    public Task DisposeAsync()
    {
        _connection.Dispose();
        return Task.CompletedTask;
    }
}
