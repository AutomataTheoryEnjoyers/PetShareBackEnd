using Microsoft.EntityFrameworkCore;

namespace Database.ValueObjects
{
    [Owned]
    public sealed class Address : IEquatable<Address>
    {
        public string Country { get; init; } = null!;
        public string Province { get; init; } = null!;
        public string City { get; init; } = null!;
        public string Street { get; init; } = null!;
        public string PostalCode { get; init; } = null!;

        public bool Equals(Address? other)
        {
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return (
                Country == other.Country &&
                Province == other.Province &&
                City == other.City &&
                Street == other.Street &&
                PostalCode == other.PostalCode);
        }
    }
}
