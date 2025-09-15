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
    [Route("api/[controller]")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        private readonly IToolServices _toolServices;

        public ToolController(IToolServices toolServices)
        {
            _toolServices = toolServices;
        }

        /// <summary>
        /// Endpoint that receives the request to return all tools paginated
        /// </summary>
        /// <param name="pagedDataDto"></param>
        /// <returns></returns>
        [HttpGet("Paged")]
        [SwaggerOperation("Endpoint that receives the request to return all tools paginated")]
        [ProducesResponseType(typeof(PagedResponseDto<ToolDto>), StatusCodes.Status200OK)]
        public IActionResult FindAllPaged([FromQuery] ToolPagedDataDto toolPagedDataDto)
        {
            var result = _toolServices.FindAllPaged(toolPagedDataDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to return all tools
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return tool types")]
        [ProducesResponseType(typeof(IEnumerable<ToolDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAll()
        {
            var result = await _toolServices.FindAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that receives the request to create a tool in the database
        /// </summary>
        /// <param name="toolCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a tool in the database")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] ToolCreateDto toolCreateDto)
        {
            var result = await _toolServices.CreateAsync(toolCreateDto);
            return Ok(result);
        }

        /// <summary>
        /// EndPoint that update a tool
        /// </summary>
        /// /// <param name="toolUpdateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("EndPoint that update a tool")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] ToolUpdateDto toolUpdateDto)
        {
            var result = await _toolServices.UpdateAsync(toolUpdateDto);
            return Ok(result);
        }

        /// <summary>
        /// EndPoint that delete tools by id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [SwaggerOperation("EndPoint that delete tools by id")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult DeleteByIds(List<int> ids)
        {
            var result = _toolServices.DeleteAsync(ids);
            return Ok(result);
        }
    }
}
