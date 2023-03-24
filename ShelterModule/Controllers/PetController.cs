using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Pets;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule.Controllers;

[ApiController]
[Route("pet")]
public class PetController : ControllerBase
{
        // dependency injection
        private readonly IPetQuery _query;
        private readonly IPetCommand _command;
        private readonly IShelterQuery _shelterQuery;

        public PetController( IPetQuery query, IPetCommand command, IShelterQuery shelterQuery)
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
        [Route("{id:guid}")]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PetResponse>> Get(Guid id)
        {
            var pet = await _query.GetByIdAsync(id);
            if (pet is null)
                return NotFound(new NotFoundResponse
                {
                    ResourceName = nameof(Pet),
                    Id = id.ToString()
                });

            return pet.ToResponse();
        }

        /// <summary>
        ///     Returns all pets for a given shelter
        /// </summary>
        /// <returns> List of all pets for a given shelter (temporary just all pets) </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
        //public async Task<IReadOnlyList<PetResponse>> GetAll()
        public async Task<ActionResult<IReadOnlyList<PetResponse>>> GetAll()
        {
            return (await _query.GetAllAsync(HttpContext.RequestAborted)).Select(s => s.ToResponse()).ToList();
        }

        /// <summary>
        ///     Creates new pet
        /// </summary>
        /// <param name="request"> Request received </param>
        /// <returns> Newly created pet </returns>
        [HttpPost]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PetResponse>> Post(PetUpsertRequest request)
        {
            // check if given shelterId is valid (TO DO: change after authorization is implemented)
            var shelter = await _shelterQuery.GetByIdAsync(request.ShelterId);
            if (shelter is null)
                return BadRequest();

            // create new pet
            var pet = Pet.FromRequest(request, shelter);
            return (await _command.AddAsync(pet)).ToResponse();
        }

        /// <summary>
        ///     Updates pet record with specified ID
        /// </summary>
        /// <param name="id"> ID of a pet to update </param>
        /// <param name="request"> Request received </param>
        /// <returns> Updated pet </returns>
        [HttpPut]
        [Route("{id:guid}")]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PetResponse>> Put(Guid id, PetUpsertRequest request)
        {
            // check if given shelterId is valid (TO DO: change after authorization is implemented)
            var shelter = await _shelterQuery.GetByIdAsync(request.ShelterId);
            if (shelter is null)
                return BadRequest();

            // update pet if there is a pet with given id
            var pet = await _command.UpdateAsync(id,request);
            if (pet is null)
                return NotFound(new NotFoundResponse
                {
                    ResourceName = nameof(Shelter),
                    Id = id.ToString()
                });

            return pet.ToResponse();
        }
    
}
