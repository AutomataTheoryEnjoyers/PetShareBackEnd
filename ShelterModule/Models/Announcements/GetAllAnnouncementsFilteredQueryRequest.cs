namespace ShelterModule.Models.Announcements;

public sealed class GetAllAnnouncementsFilteredQueryRequest
{
    public int? pageNumber { get; init; } = null;
    public int? pageCount { get; init; } = null;
    public IReadOnlyList<string>? Species { get; init; } = null;
    public IReadOnlyList<string>? Breeds { get; init; } = null;
    public IReadOnlyList<string>? Locations { get; init; } = null;
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public IReadOnlyList<string>? ShelterNames { get; init; } = null;
}
