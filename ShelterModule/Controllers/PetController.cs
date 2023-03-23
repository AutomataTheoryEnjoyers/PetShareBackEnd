using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Pets;
using ShelterModule.Models.Shelters;
using ShelterModule.Services.Interfaces.Pets;

namespace ShelterModule.Controllers
{
    [ApiController]
    [Route("pet")]
    public class PetController : ControllerBase
    {

        // dependency injection
        private readonly IPetQuery _query;
        private readonly IPetCommand _command;

        public PetController( IPetQuery query, IPetCommand command)
        {
            _query = query;
            _command = command;
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
            //var pet = await _query.GetByIdAsync(id);
            //if (pet is null)
            //    return NotFound(new NotFoundResponse
            //    {
            //        ResourceName = nameof(Pet),
            //        Id = id.ToString()
            //    });

            //return pet.ToResponse();
            await Task.CompletedTask;
            return Ok();
        }

        /// <summary>
        ///     Returns all pets for a given shelter
        /// </summary>
        /// <returns> List of all pets for a given shelter </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<PetResponse>), StatusCodes.Status200OK)]
        //public async Task<IReadOnlyList<PetResponse>> GetAll()
        public async Task<ActionResult> GetAll()
        {
            //return (await _query.GetAllAsync(HttpContext.RequestAborted)).Select(s => s.ToResponse()).ToList();
            await Task.CompletedTask;
            return Ok( new List<PetResponse>());
        }


        /// <summary>
        ///     Creates new pet
        /// </summary>
        /// <param name="request"> Request received </param>
        /// <returns> Newly created pet </returns>
        [HttpPost]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PetResponse>> Post(ShelterCreationRequest request)
        {
            //var shelter = Shelter.FromRequest(request);
            //await _command.AddAsync(shelter);
            //return shelter.ToResponse();
            await Task.CompletedTask;
            return Ok();
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
        public async Task<ActionResult<PetResponse>> Put(Guid id, PetCreationRequest request)
        {
            //var shelter = await _command.SetAuthorizationAsync(id, request.IsAuthorized);
            //if (shelter is null)
            //    return NotFound(new NotFoundResponse
            //    {
            //        ResourceName = nameof(Shelter),
            //        Id = id.ToString()
            //    });

            //return shelter.ToResponse();
            await Task.CompletedTask;
            return Ok();
        }
    }
}
