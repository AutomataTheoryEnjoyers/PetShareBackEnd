using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Adopters;
using ShelterModule.Services.Interfaces.Adopters;

namespace ShelterModule.Controllers;

[ApiController]
[Route("adopter")]
public sealed class AdopterController : ControllerBase
{
    private readonly IAdopterCommand _command;
    private readonly IAdopterQuery _query;

    public AdopterController(IAdopterQuery query, IAdopterCommand command)
    {
        _query = query;
        _command = command;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdopterResponse>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<AdopterResponse>> GetAll()
    {
        return (await _query.GetAllAsync()).Select(a => a.ToResponse()).ToList();
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdopterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdopterResponse>> Post(AdopterRequest request)
    {
        var adopter = Adopter.FromRequest(request);
        await _command.AddAsync(adopter);
        return adopter.ToResponse();
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(AdopterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdopterResponse>> Get(Guid id)
    {
        var adopter = await _query.GetByIdAsync(id);
        if (adopter is null)
            return NotFound(new NotFoundResponse
            {
                Id = id.ToString(),
                ResourceName = nameof(Adopter)
            });

        return adopter.ToResponse();
    }

    [HttpPut]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(AdopterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdopterResponse>> Put(Guid id, AdopterVerificationRequest request)
    {
        var adopter = await _command.VerifyAsync(id, request.IsVerified);
        if (adopter is null)
            return NotFound(new NotFoundResponse
            {
                Id = id.ToString(),
                ResourceName = nameof(Adopter)
            });

        return adopter.ToResponse();
    }

    [HttpPost]
    [Route("{id:guid}/authorize")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Authorize(Guid id)
    {
        var adopter = await _command.VerifyAsync(id, true);
        if (adopter is null)
            return NotFound(new NotFoundResponse
            {
                Id = id.ToString(),
                ResourceName = nameof(Adopter)
            });

        return Ok();
    }
}
