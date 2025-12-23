using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiTemplateController(IApiTemplateServices templateServices) : ControllerBase
    {
        readonly IApiTemplateServices _templateServices = templateServices;

        /// <summary>
        /// Endpoint that find all valid templates based on the passed query param filters.
        /// </summary>
        /// <param name="templateFilterDto"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that find all valid templates based on the passed query param filters")]
        [ProducesResponseType(typeof(PaginatedListDto<ApiTemplateDto>), StatusCodes.Status200OK)]
        public IActionResult FindAllPaged([FromQuery] ApiTemplatePagedFilterDto templateFilterDto)
        {
            var templates = _templateServices.FindAllPaged(templateFilterDto);
            return Ok(templates);
        }

        /// <summary>
        /// Endpoint that receive an api template id and return a valid template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("Endpoint that receive an api template id and return a valid template")]
        [ProducesResponseType(typeof(ApiTemplateDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindById(Guid id)
        {
            var workflow = await _templateServices.FindById(id);
            return Ok(workflow);
        }

        /// <summary>
        /// Delete a template by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [SwaggerOperation("Endpoint that receives the request to delete a template by its ID")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var result = await _templateServices.DeleteById(id);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that update a template.
        /// </summary>
        /// <param name="templateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("Endpoint that update a template")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] ApiTemplateUpdateDto templateDto)
        {
            var result = await _templateServices.UpdateAsync(templateDto);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint that create a new template.
        /// </summary>
        /// <param name="templateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that create a new template")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] ApiTemplateCreateDto templateDto)
        {
            var result = await _templateServices.CreateAsync(templateDto);
            return Ok(result);
        }
    }
}
