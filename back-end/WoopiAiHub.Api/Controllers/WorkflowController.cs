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
        /// Endpoint that receives a team id and returns a valid workflow
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [SwaggerOperation("Endpoint that receive an workflow id and return a valid workflow")]
        [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindById(int id, [FromQuery] WorkflowFilterDto workflowFilterDto)
        {
            var workflow = await _workflowServices.FindById(id, workflowFilterDto);
            return Ok(workflow);
        }

        /// <summary>
        /// Endpoint that receives a team id and returns a valid workflow
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Teams/{teamId}")]
        [SwaggerOperation("Endpoint that receive an team id and return a valid workflow")]
        [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindByTeamId(int teamId, [FromQuery] WorkflowFilterDto workflowFilterDto)
        {
            var workflow = await _workflowServices.FindByTeamId(teamId, workflowFilterDto);
            return Ok(workflow);
        }

        /// <summary>
        /// Delete a workflow by its ID.
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

        /// <summary>
        /// Endpoint that returns all valids workflows by user email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("Users/{email}")]
        [SwaggerOperation("Endpoint that returns all valids workflows by user email")]
        [ProducesResponseType(typeof(PagedDataDto), StatusCodes.Status200OK)]
        public ActionResult<UserPagedResultDto> FindAllByUser(string email)
        {
            var result = _workflowServices.FindAllByUser(email);
            return Ok(result);
        }

        /// <summary>
        /// Receive a page number or a search data and return
        /// workflows (with pagination)
        /// </summary>
        /// <param name="WorkflowPagedDto"></param>
        /// <returns></returns>
        [HttpGet("List")]
        [SwaggerOperation("Endpoint that returns all valids workflows by user email")]
        [ProducesResponseType(typeof(WorkflowPagedDto), StatusCodes.Status200OK)]
        public ActionResult<WorkflowPagedDto> FindAllPaged([FromQuery] WorkflowPagedDto workflowPagedDto)
        {
            var workflowList = _workflowServices.FindAllPaged(workflowPagedDto);
            return Ok(workflowList);
        }

        /// <summary>
        /// Endpoint that returns all valids workflows
        /// </summary>
        /// <param name="WorkflowPagedDto"></param>
        /// <returns></returns>
        [HttpGet()]
        [SwaggerOperation("Endpoint that returns all valids workflows")]
        [ProducesResponseType(typeof(WorkflowPagedDto), StatusCodes.Status200OK)]
        public ActionResult<WorkflowPagedDto> FindAll()
        {
            var workflowList = _workflowServices.FindAll();
            return Ok(workflowList);
        }

        /// <summary>
        /// Endpoint that returns all valids workflows
        /// </summary>
        /// <param name="WorkflowPagedDto"></param>
        /// <returns></returns>
        [HttpPut("UpdateOutput")]
        [SwaggerOperation("Endpoint that returns all valids workflows")]
        [ProducesResponseType(typeof(WorkflowPagedDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStepToolOutput([FromBody] OutputUpdateDto outputUpdateDto)
        {
            var result = await _workflowServices.UpdateStepToolOutput(outputUpdateDto);
            return Ok(result);
        }
    }
}
