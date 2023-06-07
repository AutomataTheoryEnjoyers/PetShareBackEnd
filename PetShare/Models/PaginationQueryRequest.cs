namespace PetShare.Models;

public sealed class PaginationQueryRequest
{
    public int? PageNumber { get; init; }
    public int? PageCount { get; init; }
}
