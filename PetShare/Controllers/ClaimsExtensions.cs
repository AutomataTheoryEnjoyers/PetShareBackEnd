﻿using System.Security.Claims;

namespace PetShare.Controllers;

public static class ClaimsExtensions
{
    public const string IdClaim = "db_id";

    public static Guid? TryGetId(this ClaimsPrincipal user)
    {
        var idString = user.FindFirstValue(IdClaim);
        if (idString is null)
            return null;

        return !Guid.TryParse(idString, out var id) ? null : id;
    }

    public static Guid GetId(this ClaimsPrincipal user)
    {
        return TryGetId(user) ?? throw new InvalidOperationException("JWT token does not contain ID claim");
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Admin);
    }

    public static bool IsShelter(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Shelter);
    }

    public static bool IsAdopter(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Adopter);
    }
}

public static class Roles
{
    public const string Admin = "admin";
    public const string Shelter = "shelter";
    public const string Adopter = "adopter";
    public const string Unassigned = "unassigned";

    public const string RoleClaimName = "role";
}
