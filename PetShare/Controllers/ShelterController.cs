using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models;
using PetShare.Models.Shelters;
using PetShare.Services.Interfaces.Pagination;
using PetShare.Services.Interfaces.Shelters;

namespace PetShare.Controllers;

[ApiController]
[Route("shelter")]
public sealed class ShelterController : ControllerBase
{
    private readonly IShelterCommand _command;
    private readonly IShelterQuery _query;
    private readonly IPaginationService _paginator;

    public ShelterController(IShelterQuery query, IShelterCommand command, IPaginationService paginator)
    {
        _query = query;
        _command = command;
        _paginator = paginator;
    }

    /// <summary>
    ///     Returns a shelter with a given ID
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(ShelterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShelterResponse>> Get(Guid id)
    {
        var shelter = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (shelter is null)
            return NotFound(NotFoundResponse.Shelter(id));

        return shelter.ToResponse();
    }

    /// <summary>
    ///     Returns all shelters
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedSheltersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedSheltersResponse>> GetAll([FromQuery] PaginationQueryRequest paginationQuery)
    {
        var shelters = (await _query.GetAllAsync(HttpContext.RequestAborted)).Select(s => s.ToResponse()).ToList();

        var paginatedShelters = _paginator.GetPage<ShelterResponse>(shelters, paginationQuery);

        if (paginatedShelters == null)
            return BadRequest("Wrong pagination parameters");

        return PaginatedSheltersResponse.FromPaginatedResult(paginatedShelters.Value);
    }

    /// <summary>
    ///     Creates new shelter. Requires unassigned role
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Unassigned)]
    [ProducesResponseType(typeof(ShelterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ShelterResponse>> Post(ShelterCreationRequest request)
    {
        var shelter = Shelter.FromRequest(request);
        await _command.AddAsync(shelter, HttpContext.RequestAborted);
        return shelter.ToResponse();
    }

    /// <summary>
    ///     Updates authorization status of a shelter with specified ID. Requires admin role
    /// </summary>
    [HttpPut]
    [Authorize(Roles = Roles.Admin)]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(ShelterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShelterResponse>> Put(Guid id, ShelterAuthorizationRequest request)
    {
        var shelter = await _command.SetAuthorizationAsync(id, request.IsAuthorized, HttpContext.RequestAborted);
        if (shelter is null)
            return NotFound(NotFoundResponse.Shelter(id));

        return shelter.ToResponse();
    }
}
