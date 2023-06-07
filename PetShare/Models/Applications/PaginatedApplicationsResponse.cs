using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Applications;

public class PaginatedApplicationsResponse
{
    public required IReadOnlyList<ApplicationResponse> Applications { get; init; }
    public required int PageNumber { get; init; }
    public required int Count { get; init; }

    public static PaginatedApplicationsResponse FromPaginatedResult(PaginatedResult<ApplicationResponse> result)
    {
        return new PaginatedApplicationsResponse
        {
            Applications = result.Items.ToList(),
            Count = result.TotalCount,
            PageNumber = result.PageNumber
        };
    }
}
