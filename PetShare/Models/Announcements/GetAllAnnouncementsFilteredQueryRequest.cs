namespace PetShare.Models.Announcements;

public sealed class GetAllAnnouncementsFilteredQueryRequest
{
    public IReadOnlyList<string>? Species { get; init; } = null;
    public IReadOnlyList<string>? Breeds { get; init; } = null;
    public IReadOnlyList<string>? Cities { get; init; } = null;
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public IReadOnlyList<string>? ShelterNames { get; init; } = null;
}
