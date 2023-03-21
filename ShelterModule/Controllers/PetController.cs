using Microsoft.AspNetCore.Mvc;
using ShelterModule.Models.Shelter;

namespace ShelterModule.Controllers
{
    //[ApiController]
    //[Route("shelters")]
    //public class PetController : ControllerBase
    //{

    //    // dependency injection
        
        
        
        
    //    /// <summary>
    //    ///     Returns a pet with a given ID
    //    /// </summary>
    //    /// <param name="id"> ID of the pet that should be returned </param>
    //    /// <returns> Pet with a given ID </returns>
    //    [HttpGet]
    //    [Route("{id:guid}")]
    //    [ProducesResponseType(typeof(ShelterResponse), StatusCodes.Status200OK)]
    //    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(typeof(NotFoundResponse), StatusCodes.Status404NotFound)]
    //    public async Task<ActionResult<ShelterResponse>> Get(Guid id)
    //    {
    //        var pet = await _query.GetByIdAsync(id);
    //        if (pet is null)
    //            return NotFound(new NotFoundResponse
    //            {
    //                ResourceName = nameof(Pet),
    //                Id = id.ToString()
    //            });

    //        return pet.ToResponse();
    //    }
    //}
}
