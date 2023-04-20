using System.Security.Claims;
using ShelterModule.Controllers;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Services;

public sealed class TokenValidator
{
    private readonly IShelterQuery _shelterQuery;

    public TokenValidator(IShelterQuery shelterQuery)
    {
        _shelterQuery = shelterQuery;
    }

    public async Task<TokenValidationResult> ValidateClaims(ClaimsPrincipal user)
    {
        if (user.IsInRole(Roles.Unassigned))
            return TokenValidationResult.Valid;

        if (user.IsInRole(Roles.Admin))
            return TokenValidationResult.Valid;

        if (user.IsInRole(Roles.Shelter))
        {
            var id = user.TryGetId();
            if (id is null)
                return TokenValidationResult.NoIdClaim;

            return await _shelterQuery.GetByIdAsync(id.Value) is null
                ? TokenValidationResult.InvalidId
                : TokenValidationResult.Valid;
        }

        if (user.IsInRole(Roles.Adopter))
            // TODO: Check if adopter exists
            return TokenValidationResult.Valid;

        return TokenValidationResult.InvalidRole;
    }
}

public enum TokenValidationResult
{
    Valid,
    InvalidRole,
    NoIdClaim,
    InvalidId
}
