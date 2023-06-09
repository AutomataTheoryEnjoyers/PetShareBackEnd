﻿using System.ComponentModel.DataAnnotations;
using PetShare.Models.Shelters;

namespace PetShare.Models.Pets;

public sealed class PetResponse
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string Species { get; init; }

    [Required]
    public required string Breed { get; init; }

    [Required]
    public required DateTime Birthday { get; init; }

    [Required]
    public required string Description { get; init; }

    [Required]
    public required string PhotoUrl { get; init; }

    [Required]
    public required string Status { get; init; }

    [Required]
    public required string Sex { get; init; }

    [Required]
    public required ShelterResponse Shelter { get; init; }
}
