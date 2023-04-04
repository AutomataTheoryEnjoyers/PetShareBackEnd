namespace ShelterModule.Models.Announcements;

public sealed class GetAllAnnouncementsFilteredQuery
{
    public string[]? Species { get; init; } = null!;
    public string[]? Breeds { get; init; } = null!;
    public string[]? Cities { get; init; } = null!;
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public string[]? ShelterNames { get; init; } = null!;
}
