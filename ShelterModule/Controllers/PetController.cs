using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Pets;
using ShelterModule.Services;
using ShelterModule.Services.Interfaces.Pets;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Controllers;

[ApiController]
public class PetController : ControllerBase
{
    private readonly IPetCommand _command;
    private readonly IPetQuery _query;
    private readonly IShelterQuery _shelterQuery;
    private readonly TokenValidator _validator;

    public PetController(IPetQuery query, IPetCommand command, IShelterQuery shelterQuery, TokenValidator validator)
    {
        _query = query;
        _command = command;
        _shelterQuery = shelterQuery;
        _validator = validator;
    }
    private static MultiplePetsResponse ApplyPagination(int? pageNumber, int? pageSize, List<PetResponse> pets)
    {
        if (pageNumber == null)
            pageNumber = 0;
        if (pageSize == null)
            pageSize = 10;

        if (pageNumber * pageSize > pets.Count)
            return null;

        if (pageNumber * pageSize + pageSize > pets.Count)
            pageSize = pets.Count - pageNumber * pageSize;

        return new MultiplePetsResponse
        {
            pets = pets.GetRange(pageNumber.Value * pageSize.Value, pageSize.Value),
            pageNumber = pageNumber.Value,
            count = pageSize.Value
        };
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
    [ProducesResponseType(typeof(MultiplePetsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MultiplePetsResponse>> GetAll(int? pageNumber, int? pageCount)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var allPets = (await _query.GetAllForShelterAsync(User.GetId(), HttpContext.RequestAborted)).
               Select(s => s.ToResponse()).
               ToList();

        var response = ApplyPagination(pageNumber, pageCount, allPets);
        return response == null ? BadRequest("Wrong pageNumber and pageCount parameters.") : response;
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

        var shelterObj = await _shelterQuery.GetByIdAsync(User.GetId());
        
        if(shelterObj == null)
            return Unauthorized();

        var pet = Pet.FromRequest(request, shelterObj);
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

        if (User.TryGetId() != pet.Shelter.Id)
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

        if (User.TryGetId() != pet.Shelter.Id)
            return Forbid();

        var result = await _command.SetPhotoAsync(id, file, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }
}
