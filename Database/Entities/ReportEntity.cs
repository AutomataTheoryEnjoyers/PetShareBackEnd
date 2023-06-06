using System.ComponentModel.DataAnnotations;

namespace Database.Entities;

public sealed class ReportEntity
{
    [Key]
    public required Guid Id { get; init; }

    public required Guid TargetId { get; init; }

    public required string Message { get; init; }

    public required ReportedEntityType TargetType { get; init; }

    public required ReportState State { get; set; }
}

public enum ReportedEntityType
{
    Shelter,
    Adopter,
    Announcement
}

public enum ReportState
{
    New,
    Accepted,
    Declined
}
