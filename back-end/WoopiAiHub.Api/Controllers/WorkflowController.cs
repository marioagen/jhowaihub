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
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowServices _workflowServices;

        public WorkflowController(IWorkflowServices workflowServices)
        {
            _workflowServices = workflowServices;
        }

        /// <summary>
        /// Creates a new workflow.
        /// </summary>
        /// <param name="workflowCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Endpoint that receives the request to create a workflow")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] WorkflowCreateDto workflowCreateDto)
        {
            var result = await _workflowServices.Create(workflowCreateDto);
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing workflow.
        /// </summary>
        /// <param name="workflowUpdateDto"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerOperation("Endpoint that receives the request to update a workflow")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] WorkflowUpdateDto workflowUpdateDto)
        {
             var result = await _workflowServices.Update(workflowUpdateDto);
            return Ok(result);
        }

        /// <summary>
        /// Find a workflow by its team ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Team/{id}")]
        [SwaggerOperation("Endpoint that receive an team id and return a valid workflow")]
        [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindByTeamId(int id)
        {
            var workflow = await _workflowServices.FindByTeamId(id);
            return Ok(workflow);
        }

        /// <summary>
        /// Details a workflow by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [SwaggerOperation("Endpoint that receives the request to delete a workflow by its ID")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteById(int id)
        {
            var result = await _workflowServices.DeleteById(id);
            return Ok(result);
        }
    }
}
