using System.ComponentModel.DataAnnotations;
using Database.ValueObjects;

namespace Database.Entities;

public sealed class AdopterEntity
{
    [Key]
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required bool? IsAuthorized { get; set; }
    public required string PhoneNumber { get; init; }
    public required string Email { get; init; }
    public required Address Address { get; init; }
}
