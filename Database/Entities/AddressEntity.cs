
namespace Database.Entities
{
    public sealed class AddressEntity
    {
        public string Street { get; init; } = null!;
        public string City { get; init; } = null!;
        public string Province { get; init; } = null!;
        public string PostalCode { get; init; } = null!;
        public string Country { get; init; } = null!;
    }
}
