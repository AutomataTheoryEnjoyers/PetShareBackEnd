using PetShare.Models;

namespace PetShare.Services.Interfaces.Pagination;

public interface IPaginationService
{
    PaginatedResult<T>? GetPage<T>(IReadOnlyList<T> collection, PaginationQueryRequest query);
}

public record PaginatedResult<T>(IReadOnlyList<T> Items, int PageNumber, int TotalCount);
