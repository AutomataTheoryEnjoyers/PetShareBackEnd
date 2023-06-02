using Database;
using Database.Entities;
using Database.ValueObjects;
using PetShare.Models.Adopters;
using PetShare.Models.Announcements;
using PetShare.Models.Pets;
using PetShare.Models.Shelters;
using PetShare.Results;
using PetShare.Services.Interfaces.Adopters;
using PetShare.Services.Interfaces.Shelters;

namespace PetShare.Services;

public sealed class DemoDatabasePopulator
{
    private readonly PetShareDbContext _context;
    private readonly IShelterCommand _shelterCommand;
    private readonly IShelterQuery _shelterQuery;
    private readonly IAdopterCommand _adopterCommand;
    private readonly IAdopterQuery _adopterQuery;
    private readonly Random _random = new();

    public DemoDatabasePopulator(PetShareDbContext context, IShelterCommand shelterCommand, IShelterQuery shelterQuery,
        IAdopterCommand adopterCommand, IAdopterQuery adopterQuery)
    {
        _context = context;
        _shelterQuery = shelterQuery;
        _shelterCommand = shelterCommand;
        _adopterQuery = adopterQuery;
        _adopterCommand = adopterCommand;
    }
    
    public async Task<Result> Populate()
    {
        await _adopterCommand.AddAsync(new Adopter
        {
            Id = Guid.NewGuid(),
            UserName = "adopter-1",
            Email = "mail1@email.com",
            PhoneNumber = "123456789",
            Status = AdopterStatus.Active,
            Address = GenerateAddress()
        });
    }

    private Adopter CreateAdopter()
    {
        var usernames = new[]
        {
            "69animallover69",
            "dog-collector",
            "catguru123",
            "i_miss_her",
            "theindustrialrevolutionanditsconseq",
            "tramwajeMiejskieEnjoyer"
        };

        var username = usernames[_random.Next(usernames.Length)];
        return new Adopter
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = $"{username}@testmail.test",
            PhoneNumber = $"{_random.Next(900_000_000) + 100_000_000}",
            Status = AdopterStatus.Active,
            Address = GenerateAddress()
        };
    }

    public Shelter CreateShelter()
    {
        var usernames = new(string Short, string Long)[]
        {
            ("shelter-1", "Shelter"),
            ("cat-spawn-point", "National cats reserve"),
            ("turtle_kingdom", "Turtle Kingdom"),
            ("mini-pw", "Wydział Matematyki i Nauk Informacyjnych"),
            ("weLoveDogz1234", "Best dog shelter")
        };

        var name = usernames[_random.Next(usernames.Length)];
        return new Shelter
        {
            Id = Guid.NewGuid(),
            UserName = name.Short,
            FullShelterName = name.Long,
            Email = $"{name.Short}@testmail.test",
            PhoneNumber = $"{_random.Next(900_000_000) + 100_000_000}",
            IsAuthorized = true,
            Address = GenerateAddress()
        };
    }

    public Pet CreatePet(Shelter shelter)
    {
        var pets = new (string Name, string Species, string Breed, string Description, string Photo, PetSex Sex)[]
        {
            ("Donald", "Duck", "Common", "Kinda funny",
             "https://images.fineartamerica.com/images/artworkimages/mediumlarge/3/donald-duck-michael-b-carden.jpg",
             PetSex.Male),
            ("Sasuke", "Turtle", "Rare", "Comically slow",
             "https://img.freepik.com/premium-photo/funny-turtle-underwater-showing-thumb-up_798986-902.jpg?w=2000",
             PetSex.Unknown),
            ("Crab", "Crab", "Crab", "Crab",
             "https://www.americanoceans.org/wp-content/uploads/2022/03/What-Are-The-Different-Types-of-Crab.jpg",
             PetSex.DoesNotApply),
            ("Arnold", "Cat", "Grey", "Makes weird faces which is vaguely amusing",
             "https://media.cnn.com/api/v1/images/stellar/prod/190517103414-01-grumpy-cat-file-restricted.jpg?q=x_435,y_327,h_1683,w_2244,c_crop",
             PetSex.Female),
            ("Reginald", "Dog", "Golden retriever", "Reincarnation of the king Ludvig XVI",
             "https://img.freepik.com/premium-photo/golden-retriever-dog-portrait_742252-3345.jpg?w=2000", PetSex.Male),
            ("Władysław Homenda", "Undefined", "Undefined", "Doesn't really know how he got here...",
             "https://szok.mini.pw.edu.pl/photos/homenda_wladyslaw.jpg",
             PetSex.DoesNotApply),
            ("Gilberta", "Fish", "Gold", "Loves swimming in circles (doesn't really do anything else honestly)",
             "https://pbs.twimg.com/profile_images/1319387365329293312/zPpacc_y_400x400.jpg",
             PetSex.Female)
        };

        var pet = pets[_random.Next(pets.Length)];
        return new Pet
        {
            Id = Guid.NewGuid(),
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            Description = pet.Breed,
            Photo = pet.Photo,
            Birthday = DateTime.Now - TimeSpan.FromDays(_random.Next(50 * 365) + 180),
            Shelter = shelter,
            Sex = Enum.GetValues<PetSex>()[_random.Next(Enum.GetValues<PetSex>().Length)],
            Status = PetStatus.Active
        };
    }

    public Announcement CreateAnnouncement(Pet pet)
    {
        var createdAt = DateTime.Now - TimeSpan.FromDays(_random.Next(90) + 3);
        return new Announcement
        {
            Id = Guid.NewGuid(),
            Title = $"{pet.Name} is up for adoption!",
            Description = $"We present to you: {pet.Name}!\n{pet.Description}\nPlease adopt ASAP!!!",
            CreationDate = createdAt,
            LastUpdateDate = createdAt + TimeSpan.FromDays(1),
            ClosingDate = null,
            Pet = pet,
            AuthorId = pet.Shelter.Id,
            Status = AnnouncementStatus.Open
        };
    }

    private Address GenerateAddress()
    {
        var countries = new[]
        {
            "Angola",
            "Hyrule",
            "Noxus",
            "Brazil",
            "HongKong",
            "Demacia"
        };
        var cities = new[]
        {
            "Warsaw",
            "Piltover",
            "Baku",
            "Łękołody",
            "Sosnowiec",
            "Lodz",
            "Denver",
            "Gotham",
            "Knowhere",
            "Zaun"
        };
        var provinces = countries.Concat(cities).ToArray();
        var streets = new[]
        {
            "Wall",
            "Krópówki",
            "A",
            "1",
            "The only one in the city (it's unusually tiny)",
            "Koszykowa",
            "Food"
        };
        return new Address
        {
            Country = countries[_random.Next(countries.Length)],
            Province = provinces[_random.Next(provinces.Length)],
            City = cities[_random.Next(cities.Length)],
            PostalCode = $"{_random.Next(90) + 10}-{_random.Next(900) + 100}",
            Street = streets[_random.Next(streets.Length)]
        };
    }
}
