using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileServices _profileServices;

        public ProfileController(IProfileServices profileServices)
        {
            _profileServices = profileServices;
        }

        /// <summary>
        /// Endpoint that receives the request to return a profile.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("Endpoint that receive an id and return a valid profile")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public ActionResult<ProfileDto> FindById(int id)
        {
            var team = _profileServices.FindById(id);
            return Ok(team);
        }

        /// <summary>
        /// Endpoint that receives the request to return all profiles paginated.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all profiles paginated")]
        [ProducesResponseType(typeof(ProfilePagedResultDto), StatusCodes.Status200OK)]
        public ActionResult<ProfilePagedResultDto> FindAllPaged([FromQuery] PagedDataDto pagedDataDto)
        {
            var result = _profileServices.FindAllPaged(pagedDataDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to create a profile.
        /// </summary>
        /// <param name="profileCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a profile")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] ProfileCreateDto profileCreateDto)
        {
            var result = await _profileServices.CreateUniqueProfile(profileCreateDto);
            return Ok(result);
        }

        /// <summary>
        /// EndPoint that updates a profile.
        /// </summary>
        /// <param name="profileUpdateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a profile")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] ProfileUpdateDto profileUpdateDto)
        {
            var updated = await _profileServices.Update(profileUpdateDto);
            return Ok(updated);
        }

        /// <summary>
        /// EndPoint that deletes profiles by ids.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [SwaggerOperation("EndPoint that delete profiles by ids")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult DeleteByIds(List<int> ids)
        {
            var deleted = _profileServices.DeleteByIds(ids);
            return Ok(deleted);
        }
    }
}
