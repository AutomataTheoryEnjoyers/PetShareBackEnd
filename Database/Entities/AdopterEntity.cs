using System.ComponentModel.DataAnnotations;
using Database.ValueObjects;

namespace Database.Entities;

public sealed class AdopterEntity
{
    [Key]
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }
    public required AdopterStatus Status { get; set; }
    public DateTime? DeletionTime { get; set; }
}

public enum AdopterStatus
{
    Active = 0,
    Blocked = 1,
    Deleted = 2
}
