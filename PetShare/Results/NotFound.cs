using Microsoft.AspNetCore.Mvc;
using PetShare.Models;

namespace PetShare.Results;

public record NotFound(Guid Id, string Name) : ResultState
{
    public override ActionResult ToActionResult()
    {
        return new NotFoundObjectResult(new NotFoundResponse
        {
            Id = Id,
            ResourceName = Name
        });
    }
}

public sealed record AdopterNotFound(Guid Id) : NotFound(Id, "Adopter");

public sealed record ShelterNotFound(Guid Id) : NotFound(Id, "Shelter");

public sealed record PetNotFound(Guid Id) : NotFound(Id, "Pet");

public sealed record ApplicationNotFound(Guid Id) : NotFound(Id, "Application");

public sealed record AnnouncementNotFound(Guid Id) : NotFound(Id, "Announcement");

public sealed record ReportNotFound(Guid Id) : NotFound(Id, "Report");
