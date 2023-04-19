namespace ShelterModule.Models.Announcements;

public sealed class AnnouncementPutRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public int? Status { get; init; }
}
