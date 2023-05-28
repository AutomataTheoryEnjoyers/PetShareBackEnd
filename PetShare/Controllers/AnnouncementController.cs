using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models;
using PetShare.Models.Announcements;
using PetShare.Services;
using PetShare.Services.Interfaces.Announcements;

namespace PetShare.Controllers;

[ApiController]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementCommand _command;
    private readonly IAnnouncementQuery _query;
    private readonly TokenValidator _validator;

    public AnnouncementController(IAnnouncementQuery query, IAnnouncementCommand command,
        TokenValidator validator)
    {
        _query = query;
        _command = command;
        _validator = validator;
    }

    /// <summary>
    ///     Returns an announcement with a given ID
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [Route("announcements/{id:guid}")]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnouncementResponse>> Get(Guid id)
    {
        var announcement = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (announcement is null)
            return NotFound(NotFoundResponse.Announcement(id));

        return announcement.ToResponse();
    }

    /// <summary>
    ///     Returns announcements based on filters
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [Route("announcements")]
    [ProducesResponseType(typeof(IReadOnlyList<AnnouncementResponse>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<AnnouncementResponse>> GetAllFiltered(
        [FromQuery] GetAllAnnouncementsFilteredQueryRequest query)
    {
        return (await _query.GetAllFilteredAsync(query, HttpContext.RequestAborted)).Select(s => s.ToResponse()).
                                                                                     ToList();
    }

    /// <summary>
    ///     Gets all announcements created by shelter specified in JWT claims. Requires shelter role
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Shelter)]
    [Route("shelter/announcements")]
    [ProducesResponseType(typeof(IReadOnlyList<AnnouncementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AnnouncementResponse>>> GetFromShelter()
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        return (await _query.GetForShelterAsync(User.GetId(), HttpContext.RequestAborted)).
               Select(a => a.ToResponse()).
               ToList();
    }

    /// <summary>
    ///     Creates new announcement. Requires shelter role
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Shelter)]
    [Route("announcements")]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AnnouncementResponse>> Post(AnnouncementCreationRequest request)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var announcement = Announcement.FromRequest(request, User.GetId());
        var result = await _command.AddAsync(announcement, HttpContext.RequestAborted);
        return result.HasValue ? announcement.ToResponse() : result.State.ToActionResult();
    }

    /// <summary>
    ///     Updates specified announcement. Requires shelter role
    /// </summary>
    [HttpPut]
    [Authorize(Roles = Roles.Shelter)]
    [Route("announcements/{id:guid}")]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnouncementResponse>> Put(Guid id, AnnouncementPutRequest request)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var announcement = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (announcement is null)
            return NotFound(NotFoundResponse.Announcement(id));

        if (User.GetId() != announcement.AuthorId)
            return Forbid();

        var updated = await _command.UpdateAsync(id, request, HttpContext.RequestAborted);
        if (updated is null)
            return NotFound(NotFoundResponse.Announcement(id));

        return updated.ToResponse();
    }
}
