using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Announcements;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Announcements;
using ShelterModule.Services.Interfaces.Pets;
using ShelterModule.Services.Interfaces.Shelters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShelterModule.Controllers

{
    [ApiController]
    [Route("announcements")]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementQuery _query;
        private readonly IAnnouncementCommand _command;
        private readonly IPetQuery _petQuery;
        private readonly IShelterQuery _shelterQuery;

        public AnnouncementController(IAnnouncementQuery query, IAnnouncementCommand command, IPetQuery petQuery,IShelterQuery shelterQuery)
        {
            _query = query;
            _command = command;
            _petQuery = petQuery;
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
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AnnouncementResponse>> Get(Guid id)
        {
            var announcement = await _query.GetByIdAsync(id);
            if (announcement is null)
                return NotFound(new NotFoundResponse
                {
                    ResourceName = nameof(Announcement),
                    Id = id.ToString()
                });

            return announcement.ToResponse();
        }
        
        /// /// <summary>
        ///     Returns announcements based on filters
        /// </summary>
        /// <param name="query"> query with parameters to filter announcements </param>
        /// <returns> List of all announcements matching the filters </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AnnouncementResponse>), StatusCodes.Status200OK)]        
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IReadOnlyList<AnnouncementResponse>> GetAllFiltered([FromQuery] GetAllAnnouncementsFilteredQuery query)
        {            
            return (await _query.GetAllFilteredAsync(query)).Select(s => s.ToResponse()).ToList();
        }

        /// <summary>
        ///     Creates new announcement
        /// </summary>
        /// <param name="request"> Request received </param>
        /// <returns> Newly created announcement </returns>
        [HttpPost]
        [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AnnouncementResponse>> Post(AnnouncementCreationRequest request)
        {
            // check if given shelterId is valid (TO DO: change after authorization is implemented)
            var shelter = await _shelterQuery.GetByIdAsync(request.ShelterId);
            if (shelter is null)
                return BadRequest();

            var pet = request.PetId is null ? 
                Pet.FromRequest(request.PetRequest, shelter) : await _petQuery.GetByIdAsync((Guid)request.PetId);
            if (pet is null)
                return BadRequest();
            // create new announcement
            var announcement = Announcement.FromRequest(request, shelter, pet);
            return (await _command.AddAsync(announcement)).ToResponse();
        }
        [HttpPut]
        [Route("{id:guid}")]
        [ProducesResponseType(typeof(AnnouncementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AnnouncementResponse>> Put(Guid id, AnnouncementPutRequest request)
        {
            // check if given shelterId is valid (TO DO: change after authorization is implemented)
            var shelter = await _shelterQuery.GetByIdAsync(request.ShelterId);
            if (shelter is null)
                return BadRequest();

            // update pet if there is a pet with given id
            var announcement = await _command.UpdateAsync(id, request);
            if (announcement is null)
                return NotFound(new NotFoundResponse
                {
                    ResourceName = nameof(Shelter),
                    Id = id.ToString()
                });

            return announcement.ToResponse();
        }
    }
}
