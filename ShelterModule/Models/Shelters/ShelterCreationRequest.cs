﻿using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Models.Shelters;

public sealed class ShelterCreationRequest
{
    [Required]
    public string UserName { get; init; } = null!;

    [Required]
    public string FullShelterName { get; init; } = null!;

    [Required]
    public string PhoneNumber { get; init; } = null!;

    [Required]
    public string Email { get; init; } = null!;
}
