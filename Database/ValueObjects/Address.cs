using Microsoft.EntityFrameworkCore;

namespace Database.ValueObjects
{
    public sealed class Address : ValueObject
    {
        public string Country { get; init; } = null!;
        public string Province { get; init; } = null!;
        public string City { get; init; } = null!;
        public string Street { get; init; } = null!;
        public string PostalCode { get; init; } = null!;
        

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Country;
            yield return Province;
            yield return City;
            yield return Street;
            yield return PostalCode;
        }
    }
}
