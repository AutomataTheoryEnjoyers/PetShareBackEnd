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

            List<T> result = new();

            if (pageNr < 0 || pageSize < 0)
                return null;

            if (collection.Count() > pageNr * pageSize)
                result = collection.Skip(pageNr * pageSize).Take(pageSize).ToList();

            return new PaginatedResult<T>
            {
                items = result,
                pageNr = pageNr,
                pageSize = pageSize,
                totalCount = collection.Count()
            };
        }
    }
}
