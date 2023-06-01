using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models;
using PetShare.Models.Applications;
using PetShare.Services;
using PetShare.Services.Interfaces.Announcements;
using PetShare.Services.Interfaces.Applications;

namespace PetShare.Controllers;

[ApiController]
[Route("applications")]
public sealed class ApplicationController : ControllerBase
{
    private readonly IAnnouncementQuery _announcementQuery;
    private readonly IApplicationCommand _command;
    private readonly IApplicationQuery _query;
    private readonly TokenValidator _validator;

    public ApplicationController(IApplicationQuery query, IApplicationCommand command,
        IAnnouncementQuery announcementQuery, TokenValidator validator)
    {
        _query = query;
        _announcementQuery = announcementQuery;
        _command = command;
        _validator = validator;
    }

    /// <summary>
    ///     For admin, returns all applications.
    ///     For adopter, returns all applications created by them.
    ///     For shelter, returns all applications for announcements created by this shelter.
    ///     Requires any of these roles
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin}, {Roles.Adopter}, {Roles.Shelter}")]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ApplicationResponse>>> GetAll()
    {
        if (!await _validator.ValidateClaims(User))
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

    /// <summary>
    ///     Creates an application. Requires adopter role
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Adopter)]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Post(ApplicationRequest request)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var result = await _command.CreateAsync(request.AnnouncementId, User.GetId(), HttpContext.RequestAborted);
        return result.HasValue
            ? Created(new Uri(result.Value.Id.ToString(), UriKind.Relative), result.Value.ToResponse())
            : result.State.ToActionResult();
    }

    /// <summary>
    ///     Returns application with a given ID. Requires shelter role
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Shelter)]
    [Route("{announcementId:guid}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ApplicationResponse>>> GetForAnnouncement(Guid announcementId)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var applications = await _query.GetAllForAnnouncementAsync(announcementId, HttpContext.RequestAborted);
        if (applications is null)
            return NotFound(NotFoundResponse.Announcement(announcementId));

        if ((await _announcementQuery.GetByIdAsync(announcementId, HttpContext.RequestAborted))?.AuthorId
            != User.GetId())
            return Forbid();

        return applications.Select(app => app.ToResponse()).ToList();
    }

    /// <summary>
    ///     Withdraws an application. Requires adopter role
    /// </summary>
    [HttpPut]
    [Authorize(Roles = Roles.Adopter)]
    [Route("{id:guid}/withdraw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Withdraw(Guid id)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var application = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (application is null)
            return NotFound(NotFoundResponse.Application(id));

        if (application.Adopter.Id != User.GetId())
            return Forbid();

        var result = await _command.WithdrawAsync(id, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }

    /// <summary>
    ///     Accepts an application. Requires shelter role
    /// </summary>
    [HttpPut]
    [Authorize(Roles = Roles.Shelter)]
    [Route("{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Accept(Guid id)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var application = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (application is null)
            return NotFound(NotFoundResponse.Application(id));

        if (application.Announcement.AuthorId != User.GetId())
            return Forbid();

        var result = await _command.AcceptAsync(id, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }

    /// <summary>
    ///     Accepts an application. Requires shelter role
    /// </summary>
    [HttpPut]
    [Authorize(Roles = Roles.Shelter)]
    [Route("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reject(Guid id)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var application = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (application is null)
            return NotFound(NotFoundResponse.Application(id));

        if (application.Announcement.AuthorId != User.GetId())
            return Forbid();

        var result = await _command.RejectAsync(id, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }
}
