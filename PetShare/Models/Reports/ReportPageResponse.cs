using System.ComponentModel.DataAnnotations;

namespace PetShare.Models.Reports;

public sealed class ReportPageResponse
{
    [Required]
    public required IReadOnlyList<ReportResponse> Reports { get; init; }

    [Required]
    public required int PageNumber { get; init; }

    [Required]
    public required int Count { get; init; }
}
