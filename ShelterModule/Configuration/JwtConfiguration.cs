﻿using System.ComponentModel.DataAnnotations;

namespace ShelterModule.Configuration;

public sealed class JwtConfiguration
{
    public const string SectionName = "Jwt";

    [Required]
    public required string ValidIssuer { get; init; }

    [Required]
    public required string ValidAudience { get; init; }

    [Required]
    public required string SecurityKey { get; init; }
}
