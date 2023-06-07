namespace PetShare.Models.Announcements;

public sealed class GetAllAnnouncementsFilteredQueryRequest
{
    public int? PageNumber { get; init; }
    public int? PageCount { get; init; }
    public IReadOnlyList<string>? Species { get; init; }
    public IReadOnlyList<string>? Breeds { get; init; }
    public IReadOnlyList<string>? Cities { get; init; }
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public IReadOnlyList<string>? ShelterNames { get; init; }
    public bool IsLiked { get; init; }

    public PaginationQueryRequest GetPaginationQuery()
    {
        return new PaginationQueryRequest
        {
            PageNumber = PageNumber,
            PageCount = PageCount
        };
    }
}
