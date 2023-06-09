﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models;
using PetShare.Models.Pets;
using PetShare.Services;
using PetShare.Services.Interfaces.Pagination;
using PetShare.Services.Interfaces.Pets;
using PetShare.Services.Interfaces.Shelters;

namespace PetShare.Controllers;

[ApiController]
public class PetController : ControllerBase
{
    private readonly IPetCommand _command;
    private readonly IPaginationService _paginator;
    private readonly IPetQuery _query;
    private readonly IShelterQuery _shelterQuery;
    private readonly TokenValidator _validator;

    public PetController(IPetQuery query, IPetCommand command, IShelterQuery shelterQuery, IPaginationService paginator,
        TokenValidator validator)
    {
        _query = query;
        _command = command;
        _shelterQuery = shelterQuery;
        _paginator = paginator;
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
            return NotFound(NotFoundResponse.Pet(id));

        return pet.ToResponse();
    }

    /// <summary>
    ///     Returns all pets from a given shelter. Requires shelter role
    /// </summary>
    [HttpGet]
    [Route("shelter/pets")]
    [Authorize(Roles = Roles.Shelter)]
    [ProducesResponseType(typeof(PaginatedPetsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedPetsResponse>> GetAll([FromQuery] PaginationQueryRequest paginationQuery)
    {
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var pets = (await _query.GetAllForShelterAsync(User.GetId(), HttpContext.RequestAborted)).
                   Select(s => s.ToResponse()).
                   ToList();

        var paginatedPets = _paginator.GetPage(pets, paginationQuery);
        if (paginatedPets == null)
            return BadRequest("Wrong pagination parameters");

        return PaginatedPetsResponse.FromPaginatedResult(paginatedPets);
    }

    /// <summary>
    ///     Creates new pet. Requires shelter role
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
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var shelter = await _shelterQuery.GetByIdAsync(User.GetId());

        if (shelter is null)
            return Unauthorized();

        var pet = Pet.FromRequest(request, shelter);
        await _command.AddAsync(pet, HttpContext.RequestAborted);
        return pet.ToResponse();
    }

    /// <summary>
    ///     Updates pet with specified ID. Requires shelter role
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
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var pet = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (pet is null)
            return NotFound(NotFoundResponse.Pet(id));

        if (User.TryGetId() != pet.Shelter.Id)
            return Forbid();

        var updatedPet = await _command.UpdateAsync(id, request, HttpContext.RequestAborted);
        if (updatedPet is null)
            return NotFound(NotFoundResponse.Pet(id));

        return updatedPet.ToResponse();
    }

    /// <summary>
    ///     Updates pet photo. Requires shelter role
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
        if (!await _validator.ValidateClaims(User))
            return Unauthorized();

        var pet = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (pet is null)
            return NotFound(NotFoundResponse.Pet(id));

        if (User.TryGetId() != pet.Shelter.Id)
            return Forbid();

        var result = await _command.SetPhotoAsync(id, file, HttpContext.RequestAborted);
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }
}
