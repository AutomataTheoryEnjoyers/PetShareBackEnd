using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Announcements;

public class PaginatedAnnouncementsResponse
{
    public required IReadOnlyList<AnnouncementResponse> Announcements { get; init; }
    public required int PageNumber { get; init; }
    public required int Count { get; init; }

    public static PaginatedAnnouncementsResponse FromPaginatedResult(PaginatedResult<AnnouncementResponse> result)
    {
        return new PaginatedAnnouncementsResponse
        {
            Announcements = result.Items.ToList(),
            Count = result.TotalCount,
            PageNumber = result.PageNumber
        };
    }
}

public class PaginatedLikedAnnouncementsResponse
{
    public required IReadOnlyList<LikedAnnouncementResponse> Announcements { get; init; }
    public required int PageNumber { get; init; }
    public required int Count { get; init; }

    public static PaginatedLikedAnnouncementsResponse FromPaginatedResult(
        PaginatedResult<LikedAnnouncementResponse> result)
    {
        return new PaginatedLikedAnnouncementsResponse
        {
            Announcements = result.Items.ToList(),
            Count = result.TotalCount,
            PageNumber = result.PageNumber
        };
    }
}
