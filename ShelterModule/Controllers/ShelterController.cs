using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Controllers;

[ApiController]
[Route("shelter")]
public sealed class ShelterController : ControllerBase
{
    private readonly IShelterCommand _command;
    private readonly IShelterQuery _query;

    public ShelterController(IShelterQuery query, IShelterCommand command)
    {
        _query = query;
        _command = command;
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
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Shelter),
                Id = id.ToString()
            });

        return shelter.ToResponse();
    }

    /// <summary>
    ///     Returns all shelters
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ShelterResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<MultipleSheltersResponse>> GetAll(int pageNumber, int pageCount)
    {
        if (pageNumber == null)
            pageNumber = 0;
        if (pageCount == null)
            pageCount = 10;

        var shelterPagedResponse = await _query.GetPagedAsync(pageNumber, pageCount, HttpContext.RequestAborted);

        if(shelterPagedResponse == null)
        {
            return BadRequest("Wrong pageNumber and pageCount parameters.");
        }

        var shelterList = shelterPagedResponse.Select(s => s.ToResponse()).ToList();
        return new MultipleSheltersResponse
        {
            shelters = shelterList,
            pageNumber = pageNumber,
        };
    }

    /// <summary>
    ///     Creates new shelter
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
    ///     Updates authorization status of a shelter with specified ID
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
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Shelter),
                Id = id.ToString()
            });

        return shelter.ToResponse();
    }
}
