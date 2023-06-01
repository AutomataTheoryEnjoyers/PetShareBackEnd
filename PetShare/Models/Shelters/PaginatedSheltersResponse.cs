using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Models.Shelters
{
    public sealed class PaginatedSheltersResponse
    {
        public required IReadOnlyList<ShelterResponse> Shelters { get; set; }
        public required int PageNumber { get; set; }
        public required int Count { get; set; }

        public static PaginatedSheltersResponse FromPaginatedResult(PaginatedResult<ShelterResponse> result)
        {
            return new PaginatedSheltersResponse
            {
                Shelters = result.items.ToList(),
                Count = result.totalCount,
                PageNumber = result.pageNr,
            };
        }
    }
}
