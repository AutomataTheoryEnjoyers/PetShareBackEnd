using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Applications;
using ShelterModule.Services;
using ShelterModule.Services.Interfaces.Applications;

namespace ShelterModule.Controllers;

[ApiController]
[Route("applications")]
public sealed class ApplicationController : ControllerBase
{
    private readonly IApplicationCommand _command;
    private readonly IApplicationQuery _query;
    private readonly TokenValidator _validator;

    public ApplicationController(IApplicationQuery query, IApplicationCommand command, TokenValidator validator)
    {
        _query = query;
        _command = command;
        _validator = validator;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin}, {Roles.Adopter}, {Roles.Shelter}")]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ApplicationResponse>>> GetAll()
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        if (User.IsAdmin())
            return (await _query.GetAllAsync(HttpContext.RequestAborted)).Select(app => app.ToResponse()).ToList();

        if (User.IsAdopter())
            return (await _query.GetAllForAdopterAsync(User.GetId(), HttpContext.RequestAborted)
                    ?? throw new UnreachableException()).
                   Select(app => app.ToResponse()).
                   ToList();

        if (User.IsShelter())
            return (await _query.GetAllForShelterAsync(User.GetId(), HttpContext.RequestAborted)
                    ?? throw new UnreachableException()).
                   Select(app => app.ToResponse()).
                   ToList();

        throw new UnreachableException();
    }

    [HttpPost]
    [Authorize(Roles = Roles.Adopter)]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Post(ApplicationRequest request)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var result = await _command.CreateAsync(request.AnnouncementId, User.GetId(), HttpContext.RequestAborted);
        return result.HasValue
            ? Created(new Uri(result.Value.Id.ToString(), UriKind.Relative), result.Value.ToResponse())
            : result.State.ToActionResult();
    }

    [HttpGet]
    [Authorize(Roles = Roles.Shelter)]
    [Route("{announcementId:guid}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ApplicationResponse>>> GetForAnnouncement(Guid announcementId)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var applications = await _query.GetAllForAnnouncementAsync(announcementId, HttpContext.RequestAborted);
        if (applications is null)
            return NotFound(new NotFoundResponse
            {
                Id = announcementId.ToString(),
                ResourceName = nameof(Announcement)
            });
        if (applications == null) return NotFound();

        if (applications.Count > 0 && applications[0].Announcement.AuthorId != User.GetId())
            return Forbid();

        return applications.Select(app => app.ToResponse()).ToList();
    }

    [HttpPut]
    [Authorize(Roles = Roles.Adopter)]
    [Route("{id:guid}/withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Withdraw(Guid id)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var application = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (application is null)
            return NotFound(new NotFoundResponse
            {
                Id = id.ToString(),
                ResourceName = nameof(Application)
            });

        if (application.Adopter.Id != User.GetId())
            return Forbid();

        var result = await _command.WithdrawAsync(id, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }

    [HttpPut]
    [Authorize(Roles = Roles.Shelter)]
    [Route("{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Accept(Guid id)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var application = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (application is null)
            return NotFound(new NotFoundResponse
            {
                Id = id.ToString(),
                ResourceName = nameof(Application)
            });

        if (application.Announcement.AuthorId != User.GetId())
            return Forbid();

        var result = await _command.AcceptAsync(id, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }

    [HttpPut]
    [Authorize(Roles = Roles.Shelter)]
    [Route("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reject(Guid id)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var application = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (application is null)
            return NotFound(new NotFoundResponse
            {
                Id = id.ToString(),
                ResourceName = nameof(Application)
            });

        if (application.Announcement.AuthorId != User.GetId())
            return Forbid();

        var result = await _command.RejectAsync(id, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }
}
