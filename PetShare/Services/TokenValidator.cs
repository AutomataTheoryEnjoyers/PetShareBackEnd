using System.Security.Claims;
using PetShare.Controllers;
using PetShare.Services.Interfaces.Adopters;
using PetShare.Services.Interfaces.Shelters;

namespace PetShare.Services;

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
    /// <returns> <c> true </c> if token claims are valid, <c> false </c> otherwise </returns>
    public async Task<bool> ValidateClaims(ClaimsPrincipal user)
    {
        if (user.IsInRole(Roles.Unassigned))
            return true;

        if (user.IsInRole(Roles.Admin))
            return true;

        if (user.IsInRole(Roles.Shelter))
        {
            var id = user.TryGetId();
            if (id is null)
                return false;

            return await _shelterQuery.GetByIdAsync(id.Value) is not null;
        }

        if (user.IsInRole(Roles.Adopter))
        {
            var id = user.TryGetId();
            if (id is null)
                return false;

            return await _adopterQuery.GetByIdAsync(id.Value) is not null;
        }

        return false;
    }
}
