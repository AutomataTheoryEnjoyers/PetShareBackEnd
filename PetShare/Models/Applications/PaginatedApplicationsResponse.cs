using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Applications
{
    public class PaginatedApplicationsResponse
    {
        public required IReadOnlyList<ApplicationResponse> Applications { get; set; }
        public required int PageNumber { get; set; }
        public required int Count { get; set; }
        public static PaginatedApplicationsResponse FromPaginatedResult(PaginatedResult<ApplicationResponse> result)
        {
            return new PaginatedApplicationsResponse
            {
                Applications = result.items.ToList(),
                Count = result.totalCount,
                PageNumber = result.pageNr,
            };
        }
    }
}
