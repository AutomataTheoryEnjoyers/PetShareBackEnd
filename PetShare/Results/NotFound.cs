using Microsoft.AspNetCore.Mvc;

namespace PetShare.Results;

public sealed record NotFound(Guid Id, string Name) : ResultState
{
    public override ActionResult ToActionResult()
    {
        return new NotFoundObjectResult(new NotFoundResponse
        {
            Id = Id.ToString(),
            ResourceName = Name
        });
    }
}
