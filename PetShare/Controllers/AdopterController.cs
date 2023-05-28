﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models.Adopters;
using PetShare.Services;
using PetShare.Services.Interfaces.Adopters;

namespace PetShare.Controllers;

[ApiController]
[Route("adopter")]
public sealed class AdopterController : ControllerBase
{
    private readonly IAdopterCommand _command;
    private readonly IAdopterQuery _query;
    private readonly TokenValidator _validator;

    public AdopterController(IAdopterQuery query, IAdopterCommand command, TokenValidator validator)
    {
        _query = query;
        _command = command;
        _validator = validator;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(IReadOnlyList<AdopterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AdopterResponse>>> GetAll()
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        return (await _query.GetAllAsync()).Select(a => a.ToResponse()).ToList();
    }

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

    [HttpGet]
    [Route("{id:guid}")]
    [Authorize(Roles = $"{Roles.Adopter}, {Roles.Admin}")]
    [ProducesResponseType(typeof(AdopterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdopterResponse>> Get(Guid id)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        if (!User.IsAdmin() && User.GetId() != id)
            return Forbid();

        var adopter = await _query.GetByIdAsync(id);
        if (adopter is null)
            return NotFound(NotFoundResponse.Adopter(id));

        return adopter.ToResponse();
    }

    [HttpPut]
    [Route("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(AdopterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdopterResponse>> Put(Guid id, AdopterUpdateRequest request)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var adopter = await _command.SetStatusAsync(id, request.Status);
        if (adopter is null)
            return NotFound(NotFoundResponse.Adopter(id));

        return adopter.ToResponse();
    }

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
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var result = await _command.VerifyForShelterAsync(id, User.GetId());
        return result.HasValue ? Ok() : result.State.ToActionResult();
    }

    [HttpGet]
    [Route("{id:guid}/isVerified")]
    [Authorize(Roles = Roles.Shelter)]
    [ProducesResponseType(typeof(AdopterVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdopterVerificationResponse>> IsVerified(Guid id)
    {
        if (await _validator.ValidateClaims(User) is not TokenValidationResult.Valid)
            return Unauthorized();

        var result = await _query.IsVerifiedForShelterAsync(id, User.GetId());
        if (!result.HasValue)
            return result.State.ToActionResult();

        return new AdopterVerificationResponse
        {
            IsVerified = result.Value
        };
    }
}