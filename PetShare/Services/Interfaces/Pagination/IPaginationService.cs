using PetShare.Models;

namespace PetShare.Services.Interfaces.Pagination
{
    public interface IPaginationService
    {
        public PaginatedResult<T>? GetPage<T>(IEnumerable<T> collection, PaginationQueryRequest query);
    }

    public struct PaginatedResult<T>
    {
        public IEnumerable<T> items;
        public int pageSize;
        public int pageNr;
        public int totalCount;
    }
}
