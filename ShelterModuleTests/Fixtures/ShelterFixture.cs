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
                FullShelterName = "fullshelterName1",
                IsAuthorized = null,
                PhoneNumber = "000-000-001"
            },
            new Shelter{
                Id =  Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Email = "email2",
                FullShelterName = "fullshelterName2",
                IsAuthorized = true,
                PhoneNumber = "000-000-002"
            },
            new Shelter{
                Id =  Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Email = "email3",
                FullShelterName = "fullshelterName3",
                IsAuthorized = false,
                PhoneNumber = "000-000-003"
            },

        };

        private static Shelter _testShelter = new()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000000"),
            Email = "email",
            FullShelterName = "fullshelterName",
            IsAuthorized = null,
            PhoneNumber = "000-000-000"
        };

        public static List<Shelter> GetTestShelters() => _testShelterList;

        public static Shelter GetTestShelter() => _testShelter;

    }
}
