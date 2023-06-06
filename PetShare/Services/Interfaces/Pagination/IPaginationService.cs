using PetShare.Models;

namespace PetShare.Services.Interfaces.Pagination
{
    public interface IPaginationService
    {
        PaginatedResult<T>? GetPage<T>(IEnumerable<T> collection, PaginationQueryRequest query);
    } 

    public record PaginatedResult<T>
    {
        public required IReadOnlyList<T> items;
        public required int pageSize;
        public required int pageNr;
        public required int totalCount;
    }
}
