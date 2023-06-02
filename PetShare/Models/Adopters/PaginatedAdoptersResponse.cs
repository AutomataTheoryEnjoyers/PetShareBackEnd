using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Adopters
{
    public sealed class PaginatedAdoptersResponse
    {
        public required IReadOnlyList<AdopterResponse> Adopters { get; set; }
        public required int PageNumber { get; set; }
        public required int Count { get; set; }

        public static PaginatedAdoptersResponse FromPaginatedResult(PaginatedResult<AdopterResponse> result)
        {
            return new PaginatedAdoptersResponse
            {
                Adopters = result.items.ToList(),
                Count = result.totalCount,
                PageNumber = result.pageNr,
            };
        }
    }
}
