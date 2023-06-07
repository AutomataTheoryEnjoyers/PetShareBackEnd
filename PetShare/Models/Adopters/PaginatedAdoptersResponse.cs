using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Adopters;

public sealed class PaginatedAdoptersResponse
{
    public required IReadOnlyList<AdopterResponse> Adopters { get; init; }
    public required int PageNumber { get; init; }
    public required int Count { get; init; }

    public static PaginatedAdoptersResponse FromPaginatedResult(PaginatedResult<AdopterResponse> result)
    {
        return new PaginatedAdoptersResponse
        {
            Adopters = result.Items.ToList(),
            Count = result.TotalCount,
            PageNumber = result.PageNumber
        };
    }
}
