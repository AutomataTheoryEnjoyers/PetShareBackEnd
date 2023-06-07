using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Pets;

public sealed class PaginatedPetsResponse
{
    public required IReadOnlyList<PetResponse> Pets { get; init; }
    public required int PageNumber { get; init; }
    public required int Count { get; init; }

    public static PaginatedPetsResponse FromPaginatedResult(PaginatedResult<PetResponse> result)
    {
        return new PaginatedPetsResponse
        {
            Pets = result.Items.ToList(),
            Count = result.TotalCount,
            PageNumber = result.PageNumber
        };
    }
}
