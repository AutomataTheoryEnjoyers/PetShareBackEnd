using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Pets;
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
    private static MultipleSheltersResponse ApplyPagination(int? pageNumber, int? pageSize, List<ShelterResponse> shelters)
    {
        if (pageNumber == null)
            pageNumber = 0;
        if (pageSize == null)
            pageSize = 10;

        if (pageNumber * pageSize > shelters.Count)
            return null;

        if (pageNumber * pageSize + pageSize > shelters.Count)
            pageSize = shelters.Count - pageNumber * pageSize;

        return new MultipleSheltersResponse
        {
            shelters = shelters.GetRange(pageNumber.Value * pageSize.Value, pageSize.Value),
            pageNumber = pageNumber.Value,
            count = pageSize.Value
        };
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

        var allAdopters = (await _query.GetAllAsync(HttpContext.RequestAborted)).Select(s => s.ToResponse()).ToList();

        var response = ApplyPagination(pageNumber, pageCount, allAdopters);
        return response == null ? BadRequest("Wrong pageNumber and pageCount parameters.") : response;
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
