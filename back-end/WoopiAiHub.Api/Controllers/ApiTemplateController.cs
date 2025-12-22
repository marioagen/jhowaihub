using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiTemplateController(IWorkflowServices workflowServices) : ControllerBase
    {
        readonly IWorkflowServices _workflowServices = workflowServices;

        /// <summary>
        /// Endpoint that find all valid templates based on the passed query param filters.
        /// </summary>
        /// <param name="templateFilterDto"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that find all valid templates based on the passed query param filters")]
        [ProducesResponseType(typeof(PaginatedListDto<ApiTemplateDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindAll([FromQuery] ApiTemplateFilterDto templateFilterDto)
        {
            //var workflow = await _workflowServices.FindById(templateFilterDto);
            //return Ok(workflow);
        }

        /// <summary>
        /// Endpoint that receive an api template id and return a valid template.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [SwaggerOperation("Endpoint that receive an api template id and return a valid template")]
        [ProducesResponseType(typeof(ApiTemplateDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindById(Guid id)
        {
            //var workflow = await _workflowServices.FindById(id, workflowFilterDto);
            //return Ok(workflow);
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
            //var result = await _workflowServices.DeleteById(id);
            //return Ok(result);
        }

        /// <summary>
        /// Endpoint that update a template.
        /// </summary>
        /// <param name="templateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("Endpoint that update a template")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] ApiTemplateDto templateDto)
        {
            //var result = await _workflowServices.UpdateStepToolOutput(outputUpdateDto);
            //return Ok(result);
        }

        /// <summary>
        /// Endpoint that create a new template.
        /// </summary>
        /// <param name="templateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that create a new template")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] ApiTemplateDto templateDto)
        {
            //var workflowId = await _workflowServices.CreatePhase1(workflowPhase1Dto);
            //return Ok(workflowId);
        }
    }
}
