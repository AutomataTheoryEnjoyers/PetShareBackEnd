using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models;
using PetShare.Models.Adopters;
using PetShare.Services;
using PetShare.Services.Interfaces.Adopters;
using PetShare.Services.Interfaces.Pagination;

namespace PetShare.Controllers;

[ApiController]
[Route("adopter")]
public sealed class AdopterController : ControllerBase
{
    private readonly IAdopterCommand _command;
    private readonly IPaginationService _paginator;
    private readonly IAdopterQuery _query;
    private readonly TokenValidator _validator;

    public AdopterController(IAdopterQuery query, IAdopterCommand command, IPaginationService paginator,
        TokenValidator validator)
    {
        _query = query;
        _command = command;
        _paginator = paginator;
        _validator = validator;
    }

    /// <summary>
    ///     Returns all adopters. Requires admin role
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(PaginatedAdoptersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedAdoptersResponse>> GetAll(
        [FromQuery] PaginationQueryRequest paginationQuery)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var adopters = (await _query.GetAllAsync()).Select(a => a.ToResponse()).ToList();

        var paginatedAdopters = _paginator.GetPage(adopters, paginationQuery);
        if (paginatedAdopters == null)
            return BadRequest("Wrong pagination parameters");

        return PaginatedAdoptersResponse.FromPaginatedResult(paginatedAdopters);
    }

    /// <summary>
    ///     Creates new adopter. Requires unassigned role
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Unassigned)]
    [ProducesResponseType(typeof(AdopterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdopterResponse>> Post(AdopterRequest request)
    {
        var adopter = Adopter.FromRequest(request);
        await _command.AddAsync(adopter);
        return adopter.ToResponse();
    }

    /// <summary>
    ///     Returns an adopter with given ID. Requires admin role to fetch any ID, and adopter role to fetch their ID
    /// </summary>
    [HttpGet]
    [Route("{id:guid}")]
    [Authorize(Roles = $"{Roles.Adopter}, {Roles.Admin}")]
    [ProducesResponseType(typeof(AdopterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdopterResponse>> Get(Guid id)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        if (!User.IsAdmin() && User.GetId() != id)
            return Forbid();

        var adopter = await _query.GetByIdAsync(id);
        if (adopter is null)
            return NotFound(NotFoundResponse.Adopter(id));

        return adopter.ToResponse();
    }

    /// <summary>
    ///     Updates adopter. Requires admin role
    /// </summary>
    [HttpPut]
    [Route("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(AdopterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdopterResponse>> Put(Guid id, AdopterUpdateRequest request)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var adopter = await _command.SetStatusAsync(id, request.Status);
        if (adopter is null)
            return NotFound(NotFoundResponse.Adopter(id));

        return adopter.ToResponse();
    }

    /// <summary>
    ///     Verifies adopter with ID given in route for shelter which sent the request (whose ID is in JWT claims).
    ///     Requires shelter role
    /// </summary>
    [HttpPut]
    [Route("{id:guid}/verify")]
    [Authorize(Roles = Roles.Shelter)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Verify(Guid id)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var result = await _command.VerifyForShelterAsync(id, User.GetId());
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }

    /// <summary>
    ///     Checks if adopter with ID given in route is verified for shelter whose ID is in JWT claims. Requires shelter
    ///     role
    /// </summary>
    [HttpGet]
    [Route("{id:guid}/isVerified")]
    [Authorize(Roles = Roles.Shelter)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<bool>> IsVerified(Guid id)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var result = await _query.IsVerifiedForShelterAsync(id, User.GetId());
        return result.HasValue ? result.Value : result.State.ToActionResult();
    }
}
