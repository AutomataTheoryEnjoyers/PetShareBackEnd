using Microsoft.AspNetCore.Mvc;

namespace PetShare.Results;

public abstract record ResultState
{
    public virtual ActionResult ToActionResult()
    {
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
}
