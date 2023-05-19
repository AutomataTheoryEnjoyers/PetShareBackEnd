using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models.Pets;
using PetShare.Services;
using PetShare.Services.Interfaces.Pets;

namespace PetShare.Controllers;

[ApiController]
public class PetController : ControllerBase
{
    private readonly IPetCommand _command;
    private readonly IPetQuery _query;
    private readonly TokenValidator _validator;

    public PetController(IPetQuery query, IPetCommand command, TokenValidator validator)
    {
        _query = query;
        _command = command;
        _validator = validator;
    }

    /// <summary>
    ///     Returns a pet with a given ID
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [Route("pet/{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
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
    [HttpGet]
    [Route("shelter/pets")]
    [Authorize(Roles = Roles.Shelter)]
    [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PetResponse>>> GetAll()
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        return (await _query.GetAllForShelterAsync(User.GetId(), HttpContext.RequestAborted)).
               Select(s => s.ToResponse()).
               ToList();
    }

    /// <summary>
    ///     Creates new pet
    /// </summary>
    [HttpPost]
    [Route("pet")]
    [Authorize(Roles = Roles.Shelter)]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PetResponse>> Post(PetCreationRequest request)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var pet = Pet.FromRequest(request, User.GetId());
        return (await _command.AddAsync(pet, HttpContext.RequestAborted)).ToResponse();
    }

    /// <summary>
    ///     Updates pet record with specified ID
    /// </summary>
    [HttpPut]
    [Authorize(Roles = Roles.Shelter)]
    [Route("pet/{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponse>> Put(Guid id, PetUpdateRequest request)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var pet = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (pet is null)
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Pet),
                Id = id.ToString()
            });

        if (User.TryGetId() != pet.ShelterId)
            return Forbid();

        var updatedPet = await _command.UpdateAsync(id, request, HttpContext.RequestAborted);
        if (updatedPet is null)
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Pet),
                Id = id.ToString()
            });

        return updatedPet.ToResponse();
    }

    /// <summary>
    ///     Updates pet photo
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Shelter)]
    [Route("pet/{id:guid}/photo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PostPhoto(IFormFile file, Guid id)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var pet = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (pet is null)
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Pet),
                Id = id.ToString()
            });

        if (User.TryGetId() != pet.ShelterId)
            return Forbid();

        var result = await _command.SetPhotoAsync(id, file, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }
}
