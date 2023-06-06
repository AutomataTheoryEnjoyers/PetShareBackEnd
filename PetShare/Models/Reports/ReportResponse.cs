using System.ComponentModel.DataAnnotations;

namespace PetShare.Models.Reports;

public sealed class ReportResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required Guid TargetId { get; init; }

    [Required]
    public required string Message { get; init; }

    [Required]
    public required string Type { get; init; }

    [Required]
    public required string State { get; init; }
}
