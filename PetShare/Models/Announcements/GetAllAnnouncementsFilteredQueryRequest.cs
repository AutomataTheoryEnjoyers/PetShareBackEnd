namespace PetShare.Models.Announcements;

public sealed class GetAllAnnouncementsFilteredQueryRequest
{
    public IReadOnlyList<string>? Species { get; init; }
    public IReadOnlyList<string>? Breeds { get; init; }
    public IReadOnlyList<string>? Cities { get; init; }
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public IReadOnlyList<string>? ShelterNames { get; init; }
}
