using Microsoft.EntityFrameworkCore;

namespace Database.ValueObjects;

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
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Country == other.Country
               && Province == other.Province
               && City == other.City
               && Street == other.Street
               && PostalCode == other.PostalCode;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is Address other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Country, Province, City, Street, PostalCode);
    }

    public static bool operator ==(Address? left, Address? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !Equals(left, right);
    }
}
