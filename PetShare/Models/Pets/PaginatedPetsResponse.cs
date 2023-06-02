using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Pets
{
    public sealed class PaginatedPetsResponse
    {
        public required IReadOnlyList<PetResponse> Pets { get; set; }
        public required int PageNumber { get; set; }
        public required int Count { get; set; }

        public static PaginatedPetsResponse FromPaginatedResult(PaginatedResult<PetResponse> result)
        {
            return new PaginatedPetsResponse
            {
                Pets = result.items.ToList(),
                Count = result.totalCount,
                PageNumber = result.pageNr,
            };
        }
    }
}
