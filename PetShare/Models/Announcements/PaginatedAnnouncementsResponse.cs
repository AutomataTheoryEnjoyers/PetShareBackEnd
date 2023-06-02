using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Announcements
{
    public class PaginatedAnnouncementsResponse
    {
        public required IReadOnlyList<AnnouncementResponse> Announcements { get; set; }
        public required int PageNumber { get; set; }
        public required int Count { get; set; }

        public static PaginatedAnnouncementsResponse FromPaginatedResult(PaginatedResult<AnnouncementResponse> result)
        {
            return new PaginatedAnnouncementsResponse
            {
                Announcements = result.items.ToList(),
                Count = result.totalCount,
                PageNumber = result.pageNr,
            };
        }
    }

    public class PaginatedLikedAnnouncementsResponse
    {
        public required IReadOnlyList<LikedAnnouncementResponse> Announcements { get; set; }
        public required int PageNumber { get; set; }
        public required int Count { get; set; }

        public static PaginatedLikedAnnouncementsResponse FromPaginatedResult(PaginatedResult<LikedAnnouncementResponse> result)
        {
            return new PaginatedLikedAnnouncementsResponse
            {
                Announcements = result.items.ToList(),
                Count = result.totalCount,
                PageNumber = result.pageNr,
            };
        }
    }
}
