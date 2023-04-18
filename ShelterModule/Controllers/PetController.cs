using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Pets;
using ShelterModule.Services.Interfaces.Pets;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Controllers;

[ApiController]
[Route("pet")]
public class PetController : ControllerBase
{
    private readonly IPetCommand _command;
    private readonly IPetQuery _query;
    private readonly IShelterQuery _shelterQuery;

    public PetController(IPetQuery query, IPetCommand command, IShelterQuery shelterQuery)
    {
        _query = query;
        _command = command;
        _shelterQuery = shelterQuery;
    }

    /// <summary>
    ///     Returns a pet with a given ID
    /// </summary>
    /// <param name="id"> ID of the pet that should be returned </param>
    /// <returns> Pet with a given ID </returns>
    [HttpGet]
    [AllowAnonymous]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponse>> Get(Guid id)
    {
        var pet = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (pet is null)
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Pet),
                Id = id.ToString()
            });

        return pet.ToResponse();
    }

    /// <summary>
    ///     Returns all pets
    /// </summary>
    /// <returns> List of all pets </returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<PetResponse>> GetAll()
    {
        return (await _query.GetAllAsync(HttpContext.RequestAborted)).Select(s => s.ToResponse()).ToList();
    }

    /// <summary>
    ///     Creates new pet
    /// </summary>
    /// <param name="request"> Request received </param>
    /// <returns> Newly created pet </returns>
    [HttpPost]
    [Authorize(Roles = "Shelter")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PetResponse>> Post(PetUpsertRequest request)
    {
        // TODO: Check ShelterId

        // check if shelter with a given ID exists
        var shelter = await _shelterQuery.GetByIdAsync(request.ShelterId, HttpContext.RequestAborted);
        if (shelter is null)
            return BadRequest();

        var pet = Pet.FromRequest(request);
        return (await _command.AddAsync(pet, HttpContext.RequestAborted)).ToResponse();
    }

    /// <summary>
    ///     Updates pet record with specified ID
    /// </summary>
    /// <param name="id"> ID of a pet to update </param>
    /// <param name="request"> Request received </param>
    /// <returns> Updated pet </returns>
    [HttpPut]
    [Authorize(Roles = "Shelter")]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponse>> Put(Guid id, PetUpsertRequest request)
    {
        // TODO: Check ShelterId

        var shelter = await _shelterQuery.GetByIdAsync(request.ShelterId, HttpContext.RequestAborted);
        if (shelter is null)
            return BadRequest();

        var pet = await _command.UpdateAsync(id, request, HttpContext.RequestAborted);
        if (pet is null)
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Pet),
                Id = id.ToString()
            });

        return pet.ToResponse();
    }
}
