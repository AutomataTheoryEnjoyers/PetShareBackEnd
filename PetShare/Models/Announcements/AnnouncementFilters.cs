namespace PetShare.Models.Announcements;

public sealed class AnnouncementFilters
{
    public IReadOnlyList<string>? Species { get; init; }
    public IReadOnlyList<string>? Breeds { get; init; }
    public IReadOnlyList<string>? Cities { get; init; }
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public IReadOnlyList<string>? ShelterNames { get; init; }
    public Guid? MarkLikedBy { get; init; }
    public bool IncludeOnlyLiked { get; init; }

    public static AnnouncementFilters FromRequest(GetAllAnnouncementsFilteredQueryRequest request, Guid? adopterId)
    {
        return new AnnouncementFilters
        {
            Species = request.Species,
            Breeds = request.Breeds,
            Cities = request.Cities,
            MinAge = request.MinAge,
            MaxAge = request.MaxAge,
            ShelterNames = request.ShelterNames,
            MarkLikedBy = adopterId,
            IncludeOnlyLiked = request.IsLiked
        };
    }
}
