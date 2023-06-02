using Microsoft.Extensions.Options;
using PetShare.Configuration;
using PetShare.Models;
using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Services.Implementations.Pagination
{
    public class PaginationService : IPaginationService
    {
        private readonly IOptions<PaginationConfiguration> _config;
        public PaginationService(IOptions<PaginationConfiguration> config)
        {
            _config = config;
        }

        public PaginatedResult<T>? GetPage<T>(IEnumerable<T> collection, PaginationQueryRequest query)
        {
            int pageSize = query.PageCount ?? _config.Value.DefaultPageSize;
            int pageNr = query.PageNumber ?? _config.Value.DefaultPageNumber;

            if (pageNr != 0 && collection.Count() <= pageNr * pageSize)
                return null;

            int actualElementsCount = Math.Min(pageSize, collection.Count() - pageNr * pageSize);

            return new PaginatedResult<T>
            {
                items = collection.Skip(pageNr * pageSize).Take(actualElementsCount),
                pageNr = pageNr,
                pageSize = pageSize,
                totalCount = collection.Count()
            };
        }
    }
}
