using Microsoft.AspNetCore.Mvc;

namespace ShelterModule.Results;

public sealed record InvalidOperation(string? Message = null) : ResultState
{
    public override ActionResult ToActionResult()
    {
        return Message is not null
            ? new BadRequestObjectResult(new
            {
                message = Message
            })
            : new BadRequestResult();
    }
}
