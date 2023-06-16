using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models;
using PetShare.Models.Announcements;
using PetShare.Services;
using PetShare.Services.Interfaces.Announcements;
using PetShare.Services.Interfaces.Pagination;
using PetShare.Services.Interfaces.Pets;

namespace PetShare.Controllers;

[ApiController]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementCommand _command;
    private readonly IPaginationService _paginator;
    private readonly IPetQuery _petQuery;
    private readonly IAnnouncementQuery _query;
    private readonly TokenValidator _validator;

    public AnnouncementController(IAnnouncementQuery query, IAnnouncementCommand command, IPetQuery petQuery,
        IPaginationService paginator,
        TokenValidator validator)
    {
        _query = query;
        _petQuery = petQuery;
        _command = command;
        _paginator = paginator;
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
    [ProducesResponseType(typeof(PaginatedLikedAnnouncementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedLikedAnnouncementsResponse>> GetAllFiltered(
        [FromQuery] GetAllAnnouncementsFilteredQueryRequest query)
    {
        if (User.IsAdopter() && !await _validator.ValidateClaims(User))
            return Unauthorized();

        var likedAnnouncements =
            (await
                _query.GetAllFilteredAsync(AnnouncementFilters.FromRequest(query, User.IsAdopter() ? User.GetId() : null),
                                           HttpContext.RequestAborted)).Select(s => s.ToResponse()).
                                                                        ToList();

        var paginatedLikedAnnouncements = _paginator.GetPage(likedAnnouncements, query.GetPaginationQuery());
        if (paginatedLikedAnnouncements is null)
            return BadRequest("Wrong pagination parameters");

        return PaginatedLikedAnnouncementsResponse.FromPaginatedResult(paginatedLikedAnnouncements);
    }

    /// <summary>
    ///     Gets all announcements created by shelter specified in JWT claims. Requires shelter role
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Shelter)]
    [Route("shelter/announcements")]
    [ProducesResponseType(typeof(PaginatedAnnouncementsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedAnnouncementsResponse>> GetFromShelter(
        [FromQuery] PaginationQueryRequest paginationQuery)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var announcements = (await _query.GetForShelterAsync(User.GetId(), HttpContext.RequestAborted)).
                            Select(a => a.ToResponse()).
                            ToList();

        var paginatedAnnouncements = _paginator.GetPage(announcements, paginationQuery);
        if (paginatedAnnouncements is null)
            return BadRequest("Wrong pagination parameters");

        return PaginatedAnnouncementsResponse.FromPaginatedResult(paginatedAnnouncements);
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
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var pet = await _petQuery.GetByIdAsync(request.PetId, HttpContext.RequestAborted);
        if (pet is null)
            return BadRequest();

        var announcement = Announcement.FromRequest(request, User.GetId(), pet);
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
        if (!await _validator.ValidateClaims(User))
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

    [HttpPut]
    [Authorize(Roles = Roles.Adopter)]
    [Route("announcements/{id:guid}/like")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Like(Guid id, [FromQuery] bool isLiked)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var result = await _command.LikeAsync(id, User.GetId(), isLiked, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }
}
