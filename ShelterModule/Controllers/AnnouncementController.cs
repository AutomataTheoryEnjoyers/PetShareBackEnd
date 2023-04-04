using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Pets;
using ShelterModule.Services.Interfaces.Announcements;
using ShelterModule.Services.Interfaces.Pets;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Controllers;

[ApiController]
[Route("announcements")]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementCommand _command;
    private readonly IPetCommand _petCommand;
    private readonly IPetQuery _petQuery;
    private readonly IAnnouncementQuery _query;
    private readonly IShelterQuery _shelterQuery;

    public AnnouncementController(IAnnouncementQuery query, IAnnouncementCommand command, IPetQuery petQuery,
        IPetCommand petCommand, IShelterQuery shelterQuery)
    {
        _query = query;
        _command = command;
        _petQuery = petQuery;
        _petCommand = petCommand;
        _shelterQuery = shelterQuery;
    }

    /// <summary>
    ///     Returns an announcement with a given ID
    /// </summary>
    /// <param name="id"> ID of the announcement that should be returned </param>
    /// <returns> Announcement with a given ID </returns>
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnouncementResponse>> Get(Guid id)
    {
        var announcement = await _query.GetByIdAsync(id, HttpContext.RequestAborted);
        if (announcement is null)
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Announcement),
                Id = id.ToString()
            });

        return announcement.ToResponse();
    }

    /// <summary>
    ///     Returns announcements based on filters
    /// </summary>
    /// <param name="query"> query with parameters to filter announcements </param>
    /// <returns> List of all announcements matching the filters </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AnnouncementResponse>), StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<AnnouncementResponse>> GetAllFiltered(
        [FromQuery] GetAllAnnouncementsFilteredQueryRequest query)
    {
        return (await _query.GetAllFilteredAsync(query, HttpContext.RequestAborted)).Select(s => s.ToResponse()).
                                                                                     ToList();
    }

    /// <summary>
    ///     Creates new announcement
    /// </summary>
    /// <param name="request"> Request received </param>
    /// <returns> Newly created announcement </returns>
    [HttpPost]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnnouncementResponse>> Post(AnnouncementCreationRequest request)
    {
        var shelter = await _shelterQuery.GetByIdAsync(request.ShelterId, HttpContext.RequestAborted);
        if (shelter is null)
            return BadRequest();

        var pet = request.PetId is null
            ? Pet.FromRequest(request.PetRequest ?? 
            throw new ArgumentException("PetId and PetRequest were null in AnnouncementCreationRequest"))
            : await _petQuery.GetByIdAsync(request.PetId.Value, HttpContext.RequestAborted);
        if (pet is null)
            return BadRequest();

        if (request.PetId is null)
            await _petCommand.AddAsync(pet, HttpContext.RequestAborted);

        var announcement = Announcement.FromRequest(request, pet.Id);
        return (await _command.AddAsync(announcement, HttpContext.RequestAborted)).ToResponse();
    }

    [HttpPut]
    [Route("{id:guid}")]
    [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnnouncementResponse>> Put(Guid id, AnnouncementPutRequest request)
    {
        if (request.PetId.HasValue)
        {
            var pet = await _petQuery.GetByIdAsync(request.PetId.Value, HttpContext.RequestAborted);
            if (pet is null)
                return BadRequest();
        }
        var announcement = await _command.UpdateAsync(id, request, HttpContext.RequestAborted);
        if (announcement is null)
            return NotFound(new NotFoundResponse
            {
                ResourceName = nameof(Announcement),
                Id = id.ToString()
            });

        return announcement.ToResponse();
    }
}
