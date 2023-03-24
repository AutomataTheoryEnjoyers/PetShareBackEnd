using Database.ValueObjects;
using ShelterModule.Models.Shelters;

namespace ShelterModuleTests.Fixtures
{
    public static class ShelterFixture
    {
        private static List<Shelter> _testShelterList = new()
        {
            new Shelter{
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Email = "email1",
                UserName = "shelter1",
                FullShelterName = "fullshelterName1",
                IsAuthorized = null,
                PhoneNumber = "000-000-001",
                Address = new Address
                {
                   Country = "country1",
                   Province = "province1",
                   City = "city1",
                   Street = "street1",
                   PostalCode = "postalcode1"
                }
            },
            new Shelter{
                Id =  Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Email = "email2",
                UserName = "shelter2",
                FullShelterName = "fullshelterName2",
                IsAuthorized = true,
                PhoneNumber = "000-000-002",
                Address = new Address
                {
                   Country = "country2",
                   Province = "province2",
                   City = "city2",
                   Street = "street2",
                   PostalCode = "postalcode2"
                }
            },
            new Shelter{
                Id =  Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Email = "email3",
                UserName = "shelter3",
                FullShelterName = "fullshelterName3",
                IsAuthorized = false,
                PhoneNumber = "000-000-003",
                Address = new Address
                {
                   Country = "country3",
                   Province = "province3",
                   City = "city3",
                   Street = "street3",
                   PostalCode = "postalcode3"
                }
            },

        };

        private static Shelter _testShelter = new()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000000"),
            Email = "email",
            UserName = "shelter",
            FullShelterName = "fullshelterName",
            IsAuthorized = null,
            PhoneNumber = "000-000-000",
            Address = new Address
            {
                Country = "country",
                Province = "province",
                City = "city",
                Street = "street",
                PostalCode = "postalcode"
            }
        };

        public static List<Shelter> GetTestShelters() => _testShelterList;

        public static Shelter GetTestShelter() => _testShelter;

    }
}
