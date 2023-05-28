using System.ComponentModel.DataAnnotations;

namespace PetShare;

public sealed class NotFoundResponse
{
    [Required]
    public required string ResourceName { get; init; }

    [Required]
    public required Guid Id { get; init; }

    public static NotFoundResponse Adopter(Guid id)
    {
        return new NotFoundResponse
        {
            Id = id,
            ResourceName = "Adopter"
        };
    }

    public static NotFoundResponse Shelter(Guid id)
    {
        return new NotFoundResponse
        {
            Id = id,
            ResourceName = "Shelter"
        };
    }

    public static NotFoundResponse Application(Guid id)
    {
        return new NotFoundResponse
        {
            Id = id,
            ResourceName = "Application"
        };
    }

    public static NotFoundResponse Pet(Guid id)
    {
        return new NotFoundResponse
        {
            Id = id,
            ResourceName = "Pet"
        };
    }

    public static NotFoundResponse Announcement(Guid id)
    {
        return new NotFoundResponse
        {
            Id = id,
            ResourceName = "Announcement"
        };
    }
}
