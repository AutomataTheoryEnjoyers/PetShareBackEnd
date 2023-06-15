using System.ComponentModel.DataAnnotations;
using Database.Entities;

namespace PetShare.Models.Announcements;

public sealed class GetAllAnnouncementsFilteredQueryRequest : IValidatableObject
{
    public int? PageNumber { get; init; }
    public int? PageCount { get; init; }
    public string? Status { get; init; }
    public IReadOnlyList<string>? Species { get; init; }
    public IReadOnlyList<string>? Breeds { get; init; }
    public IReadOnlyList<string>? Cities { get; init; }
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public IReadOnlyList<string>? ShelterNames { get; init; }
    public bool IsLiked { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status is not null && !Enum.TryParse<AnnouncementStatus>(Status, true, out _))
            yield return new ValidationResult($"'{Status}' is not a valid announcement status",
                                              new[] { nameof(Status) });
    }

    public PaginationQueryRequest GetPaginationQuery()
    {
        return new PaginationQueryRequest
        {
            PageNumber = PageNumber,
            PageCount = PageCount
        };
    }
}
