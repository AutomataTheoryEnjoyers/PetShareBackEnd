using System.Security.Claims;
using ShelterModule.Controllers;
using ShelterModule.Services.Interfaces.Adopters;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Services;

public sealed class TokenValidator
{
    private readonly IAdopterQuery _adopterQuery;
    private readonly IShelterQuery _shelterQuery;

    public TokenValidator(IShelterQuery shelterQuery, IAdopterQuery adopterQuery)
    {
        _shelterQuery = shelterQuery;
        _adopterQuery = adopterQuery;
    }

    /// <summary>
    ///     Performs additional validation on user claims, i.e. if the ID of user claiming to be shelter corresponds to
    ///     an existing shelter
    /// </summary>
    /// <param name="user"> <see cref="ClaimsPrincipal" /> containing user claims </param>
    /// <returns> <see cref="TokenValidationResult" /> with validation result </returns>
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
        {
            var id = user.TryGetId();
            if (id is null)
                return TokenValidationResult.NoIdClaim;

            return await _adopterQuery.GetByIdAsync(id.Value) is null
                ? TokenValidationResult.InvalidId
                : TokenValidationResult.Valid;
        }

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
