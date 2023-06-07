using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Shelters;

public sealed class PaginatedSheltersResponse
{
    public required IReadOnlyList<ShelterResponse> Shelters { get; init; }
    public required int PageNumber { get; init; }
    public required int Count { get; init; }

    public static PaginatedSheltersResponse FromPaginatedResult(PaginatedResult<ShelterResponse> result)
    {
        return new PaginatedSheltersResponse
        {
            Shelters = result.Items.ToList(),
            Count = result.TotalCount,
            PageNumber = result.PageNumber
        };
    }
}
