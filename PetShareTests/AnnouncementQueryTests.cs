﻿using Database;
using Database.Entities;
using Database.ValueObjects;
using FluentAssertions;
using PetShare.Models.Announcements;
using PetShare.Services.Implementations.Announcements;
using Xunit;

namespace PetShareTests;

[Trait("Category", "Unit")]
public sealed class AnnouncementQueryTests : IAsyncLifetime
{
    private readonly IReadOnlyList<AdopterEntity> _adopters;
    private readonly IReadOnlyList<AnnouncementEntity> _announcements;
    private readonly TestDbConnectionString _connection;
    private readonly PetShareDbContext _context;
    private readonly IReadOnlyList<PetEntity> _pets;
    private readonly AnnouncementQuery _query;
    private readonly IReadOnlyList<ShelterEntity> _shelters;

    public AnnouncementQueryTests()
    {
        _connection = IntegrationTestSetup.CreateTestDatabase();
        _context = IntegrationTestSetup.CreateDbContext(_connection);
        _query = new AnnouncementQuery(_context);

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
            },
            new ShelterEntity
            {
                Id = Guid.NewGuid(),
                UserName = "shelter-1",
                FullShelterName = "Shelter 1",
                Email = "shelter-1@mail.com",
                PhoneNumber = "634889235",
                IsAuthorized = true,
                Address = new Address
                {
                    Country = "Angola",
                    City = "Casablanca",
                    PostalCode = "12-456",
                    Province = "Nagasaki",
                    Street = "No. 1"
                }
            },
            new ShelterEntity
            {
                Id = Guid.NewGuid(),
                UserName = "shelter2",
                FullShelterName = "Shelter 2",
                Email = "shelter2@mail.com",
                PhoneNumber = "264917893",
                IsAuthorized = true,
                Address = new Address
                {
                    Country = "Armenia",
                    City = "Brazil",
                    PostalCode = "45-454",
                    Province = "Armenia",
                    Street = "Wear"
                }
            },
            new ShelterEntity
            {
                Id = Guid.NewGuid(),
                UserName = "shelter3",
                FullShelterName = "Shelter 3",
                Email = "shelter3@mail.com",
                PhoneNumber = "729837501",
                IsAuthorized = false,
                Address = new Address
                {
                    Country = "Netherlands",
                    City = "New Mexico",
                    PostalCode = "66-666",
                    Province = "Albuquerque",
                    Street = "Saul"
                }
            },
            new ShelterEntity
            {
                Id = Guid.NewGuid(),
                UserName = "shelter4",
                FullShelterName = "Shelter 4",
                Email = "shelter4@mail.com",
                PhoneNumber = "898989898",
                IsAuthorized = null,
                Address = new Address
                {
                    Country = "England",
                    City = "Lodz",
                    PostalCode = "78-121",
                    Province = "Greece",
                    Street = "Lubliana"
                }
            }
        };
        _pets = _shelters.SelectMany(GeneratePets).ToList();
        _announcements = _pets.SelectMany(CreateManyAnnouncements).ToList();

        _adopters = new[]
        {
            new AdopterEntity
            {
                Id = Guid.NewGuid(),
                UserName = "adopter-1",
                Email = "69mr-destruction69@gmail.com",
                PhoneNumber = "678456333",
                Status = AdopterStatus.Active,
                Address = new Address
                {
                    Country = "Georgia",
                    City = "Sarayevo",
                    PostalCode = "69-420",
                    Province = "South Park",
                    Street = "Food"
                }
            },
            new AdopterEntity
            {
                Id = Guid.NewGuid(),
                UserName = "adopter-2",
                Email = "hhhhhhh@gmail.com",
                PhoneNumber = "668099282",
                Status = AdopterStatus.Active,
                Address = new Address
                {
                    Country = "Roman Empire",
                    City = "Rome",
                    PostalCode = "RO-MEE",
                    Province = "Romania",
                    Street = "Roman"
                }
            }
        };
    }

    public async Task InitializeAsync()
    {
        _context.Shelters.AddRange(_shelters);
        _context.Pets.AddRange(_pets);
        _context.Announcements.AddRange(_announcements);
        _context.Adopters.AddRange(_adopters);
        _context.Likes.AddRange(new LikedAnnouncementEntity
        {
            AdopterId = _adopters[0].Id,
            AnnouncementId = _announcements.Where(a => a.Status == (int)AnnouncementStatus.Open).
                                            First(a => a.PetId == _pets.First(p => p.Name == "Reginald").Id).
                                            Id
        }, new LikedAnnouncementEntity
        {
            AdopterId = _adopters[1].Id,
            AnnouncementId = _announcements.Where(a => a.Status == (int)AnnouncementStatus.Open).
                                            Last(a => a.PetId == _pets.First(p => p.Name == "Amy").Id).
                                            Id
        });
        await _context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _connection.Dispose();
        return Task.CompletedTask;
    }

    private static AnnouncementWithLike ToLikedAnnouncement(AnnouncementEntity entity)
    {
        return new AnnouncementWithLike(Announcement.FromEntity(entity), false);
    }

    private static IReadOnlyList<PetEntity> GeneratePets(ShelterEntity shelter)
    {
        return new[]
        {
            new PetEntity
            {
                Id = Guid.NewGuid(),
                ShelterId = shelter.Id,
                Name = "Reginald",
                Description = "Reincarnation of king Ludvig XVI",
                Birthday = new DateTime(2022, 2, 22),
                Species = "Dog",
                Breed = "Golden retriever",
                Sex = PetSex.Male,
                Photo = "https://www.example.com/image.jpg",
                Status = PetStatus.Active
            },
            new PetEntity
            {
                Id = Guid.NewGuid(),
                ShelterId = shelter.Id,
                Name = "Anzlem",
                Description = "Weird",
                Birthday = new DateTime(2013, 2, 22),
                Species = "Cat",
                Breed = "Grey",
                Sex = PetSex.Male,
                Photo = "https://www.example.com/image.jpg",
                Status = PetStatus.Active
            },
            new PetEntity
            {
                Id = Guid.NewGuid(),
                ShelterId = shelter.Id,
                Name = "Xerehlapth",
                Description = "Don't look directly at it",
                Birthday = new DateTime(10, 2, 22),
                Species = "Unknown",
                Breed = "Unknown",
                Sex = PetSex.Unknown,
                Photo = "https://www.example.com/image.jpg",
                Status = PetStatus.Active
            },
            new PetEntity
            {
                Id = Guid.NewGuid(),
                ShelterId = shelter.Id,
                Name = "Elise",
                Description = "No longer with us :(",
                Birthday = new DateTime(2023, 2, 22),
                Species = "Fish",
                Breed = "Goldfish",
                Sex = PetSex.Female,
                Photo = "https://www.example.com/image.jpg",
                Status = PetStatus.Deleted
            },
            new PetEntity
            {
                Id = Guid.NewGuid(),
                ShelterId = shelter.Id,
                Name = "Amy",
                Description = "Comically slow",
                Birthday = new DateTime(2002, 2, 22),
                Species = "Turtle",
                Breed = "Grey",
                Sex = PetSex.Female,
                Photo = "https://www.example.com/image.jpg",
                Status = PetStatus.Active
            }
        };
    }

    private static AnnouncementEntity CreateAnnouncementForPet(PetEntity pet, AnnouncementStatus status)
    {
        return new AnnouncementEntity
        {
            Id = Guid.NewGuid(),
            CreationDate = new DateTime(2023, 5, 10),
            LastUpdateDate = new DateTime(2023, 5, 16),
            ClosingDate = status is (AnnouncementStatus.Closed or AnnouncementStatus.Deleted)
                ? new DateTime(2023, 5, 19)
                : null,
            AuthorId = pet.ShelterId,
            PetId = pet.Id,
            Title = $"{pet.Name} is up for adoption!",
            Description = $"Here's {pet.Name}'s description: {pet.Description}. Please adopt ASAP!",
            Status = status
        };
    }

    private IReadOnlyList<AnnouncementEntity> CreateManyAnnouncements(PetEntity pet)
    {
        return new[]
        {
            CreateAnnouncementForPet(pet, AnnouncementStatus.Open),
            CreateAnnouncementForPet(pet, AnnouncementStatus.Closed),
            CreateAnnouncementForPet(pet, AnnouncementStatus.Deleted)
        };
    }

    [Fact]
    public async Task ShouldReturnAllWithNoFilters()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters());
        result.Should().
               BeEquivalentTo(_announcements.Select(ToLikedAnnouncement),
                              options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldFilterByStatus()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            MinAge = 3,
            MaxAge = 50,
            Status = AnnouncementStatus.Open
        });
        result.Should().
               BeEquivalentTo(_announcements.Select(ToLikedAnnouncement).
                                             Where(a => a.Announcement.Status is AnnouncementStatus.Open).
                                             Where(a => DateTime.Now.AddYears(-3) >= a.Announcement.Pet.Birthday).
                                             Where(a => DateTime.Now.AddYears(-50) <= a.Announcement.Pet.Birthday),
                              options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldFilterBySpecies()
    {
        var species = new[] { "Dog", "Cat" };
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            Species = species
        });
        result.Should().
               BeEquivalentTo(_announcements.Select(ToLikedAnnouncement).
                                             Where(a => species.Contains(a.Announcement.Pet.Species)),
                              options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldFilterByBreed()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            Breeds = new[] { "Grey" }
        });
        result.Should().
               BeEquivalentTo(_announcements.Select(ToLikedAnnouncement).
                                             Where(a => a.Announcement.Pet.Breed == "Grey"),
                              options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldFilterByAge()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            MinAge = 3,
            MaxAge = 50
        });
        result.Should().
               BeEquivalentTo(_announcements.Select(ToLikedAnnouncement).
                                             Where(a => DateTime.Now.AddYears(-3) >= a.Announcement.Pet.Birthday).
                                             Where(a => DateTime.Now.AddYears(-50) <= a.Announcement.Pet.Birthday),
                              options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldFilterByCities()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            Cities = new[] { "Brazil" }
        });
        result.Should().
               BeEquivalentTo(_announcements.Select(ToLikedAnnouncement).
                                             Where(a => a.Announcement.Pet.Shelter.Address.City == "Brazil"),
                              options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldFilterByShelterNames()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            ShelterNames = new[] { "Shelter 1" }
        });
        result.Should().
               BeEquivalentTo(_announcements.Select(ToLikedAnnouncement).
                                             Where(a => a.Announcement.Pet.Shelter.FullShelterName == "Shelter 1"),
                              options => options.WithoutStrictOrdering());
    }

    [Fact]
    public async Task ShouldReturnEmptyListIfNothingMatches()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            ShelterNames = new[] { "Shelter 8" }
        });
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldReturnInfoAboutLikes()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            MarkLikedBy = _adopters[0].Id
        });
        result.Should().HaveCount(_announcements.Count());
        result.Single(a => a.IsLiked).Announcement.Pet.Id.Should().Be(_pets[0].Id);
    }

    [Fact]
    public async Task ShouldFilterByLiked()
    {
        var result = await _query.GetAllFilteredAsync(new AnnouncementFilters
        {
            MarkLikedBy = _adopters[0].Id,
            IncludeOnlyLiked = true
        });
        result.Should().HaveCount(1);
        result[0].Announcement.Pet.Id.Should().Be(_pets[0].Id);
        result[0].IsLiked.Should().BeTrue();
    }
}
