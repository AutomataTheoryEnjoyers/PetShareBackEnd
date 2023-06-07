using Microsoft.Extensions.Options;
using PetShare.Configuration;
using PetShare.Models;
using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Services.Implementations.Pagination;

public class PaginationService : IPaginationService
{
    private readonly IOptions<PaginationConfiguration> _config;

    public PaginationService(IOptions<PaginationConfiguration> config)
    {
        _config = config;
    }

    public PaginatedResult<T>? GetPage<T>(IReadOnlyList<T> collection, PaginationQueryRequest query)
    {
        var pageSize = query.PageCount ?? _config.Value.DefaultPageSize;
        var pageNr = query.PageNumber ?? _config.Value.DefaultPageNumber;

        if (pageNr < 0 || pageSize <= 0)
            return null;

        var result = collection.Skip(pageNr * pageSize).Take(pageSize).ToList();
        return new PaginatedResult<T>(result, pageNr, collection.Count);
    }
}
