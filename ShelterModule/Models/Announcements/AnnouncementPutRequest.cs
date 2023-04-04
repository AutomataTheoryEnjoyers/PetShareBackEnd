namespace ShelterModule.Models.Announcements;

public sealed class AnnouncementPutRequest
{
    public string? Title { get; init; } = null!;

    public string? Description { get; init; }

    public Guid? PetId { get; init; }

    public Guid? ShelterId { get; init; }

    public int? Status { get; init; }
}
