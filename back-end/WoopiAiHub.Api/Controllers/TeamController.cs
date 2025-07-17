using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamServices _teamServices;

        public TeamController(ITeamServices teamServices)
        {
            _teamServices = teamServices;
        }

        /// <summary>
        /// Endpoint that receives the request to return a team.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("Endpoint that receive an id and return a valid team")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public ActionResult<TeamDto> FindById(int id)
        {
            var team = _teamServices.FindById(id);
            return Ok(team);
        }

        /// <summary>
        /// Endpoint that receives the request to return all teams paginated.
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all teams paginated")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public ActionResult<PagedDataDto> FindAllPaged([FromQuery] PagedDataDto pagedDataDto)
        {
            var result = _teamServices.FindAllPaged(pagedDataDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to create a team.
        /// </summary>
        /// <param name="teamCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a team")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Create([FromBody] TeamCreateDto teamCreateDto)
        {
            var result = _teamServices.CreateUniqueTeam(teamCreateDto);
            return Ok(result);
        }

        /// <summary>
        /// EndPoint that updates a team.
        /// </summary>
        /// <param name="teamUpdateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a team")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] TeamUpdateDto teamUpdateDto)
        {
            var updated = await _teamServices.Update(teamUpdateDto);
            return Ok(updated);
        }

        /// <summary>
        /// EndPoint that deletes teams by id.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("DeleteByIds")]
        [SwaggerOperation("EndPoint that delete teams by id")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Delete(List<int> ids)
        {
            var deleted = _teamServices.DeleteByIds(ids);
            return Ok(deleted);
        }
    }
}
