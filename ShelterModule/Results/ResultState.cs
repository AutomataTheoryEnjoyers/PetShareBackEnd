using Microsoft.AspNetCore.Mvc;

namespace ShelterModule.Results;

public abstract record ResultState
{
    public virtual ActionResult ToActionResult()
    {
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
}
