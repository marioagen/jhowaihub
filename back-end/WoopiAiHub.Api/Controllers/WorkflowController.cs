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

        /// <summary>
        /// Phase 1: Creates a workflow with name and team associations.
        /// </summary>
        /// <param name="workflowPhase1Dto"></param>
        /// <returns>The ID of the created workflow</returns>
        [HttpPost("Phase1")]
        [SwaggerOperation("Endpoint for Phase 1 of workflow creation: Name and Team Associations")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePhase1([FromBody] WorkflowPhase1Dto workflowPhase1Dto)
        {
            var workflowId = await _workflowServices.CreatePhase1(workflowPhase1Dto);
            return Ok(workflowId);
        }

        /// <summary>
        /// Phase 1: Updates workflow with teams ans name information.
        /// </summary>
        /// <param name="WorkflowUpdatePhase1Dto"></param>
        /// <returns></returns>
        [HttpPut("Phase1")]
        [SwaggerOperation("Endpoint for Phase 2 of workflow creation: Steps Management")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePhase1([FromBody] WorkflowUpdatePhase1Dto workflowUpdatePhase1Dto)
        {
            var result = await _workflowServices.UpdatePhase1(workflowUpdatePhase1Dto);
            return Ok(result);
        }

        /// <summary>
        /// Phase 2: Updates workflow with steps information.
        /// </summary>
        /// <param name="workflowPhase2Dto"></param>
        /// <returns></returns>
        [HttpPut("Phase2")]
        [SwaggerOperation("Endpoint for Phase 2 of workflow creation: Steps Management")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePhase2([FromBody] WorkflowPhase2Dto workflowPhase2Dto)
        {
            var result = await _workflowServices.UpdatePhase2(workflowPhase2Dto);
            return Ok(result);
        }

        /// <summary>
        /// Phase 3: Updates workflow steps with tool flows.
        /// </summary>
        /// <param name="workflowPhase3Dto"></param>
        /// <returns></returns>
        [HttpPut("Phase3")]
        [SwaggerOperation("Endpoint for Phase 3 of workflow creation: Tool Flows Configuration")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePhase3([FromBody] WorkflowPhase3Dto workflowPhase3Dto)
        {
            var result = await _workflowServices.UpdatePhase3(workflowPhase3Dto);
            return Ok(result);
        }

        /// <summary>
        /// Get phase 3 data from id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Phase3/{id}")]
        [SwaggerOperation("Endpoint for Phase 3 of workflow creation: Tool Flows Configuration")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindPhase3ById(int id)
        {
            var result =  await _workflowServices.FindPhase3ById(id);
            return Ok(result);
        }

        /// <summary>
        /// Get phase 2 data from id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Phase2/{id}")]
        [SwaggerOperation("Endpoint for Phase 3 of workflow creation: Tool Flows Configuration")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindPhase2ById(int id)
        {
            var result =  await _workflowServices.FindPhase2ById(id);
            return Ok(result);
        }

        /// <summary>
        /// Get phase 1 data from id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Phase1/{id}")]
        [SwaggerOperation("Endpoint for Phase 3 of workflow creation: Tool Flows Configuration")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindPhase1ById(int id)
        {
            var result =  await _workflowServices.FindPhase1ById(id);
            return Ok(result);
        }

        /// <summary>
        /// Get step data from id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Step/{id}")]
        [SwaggerOperation("Endpoint for Phase 3 of workflow creation: Tool Flows Configuration")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult FindStepById(int id)
        {
            var result = _workflowServices.FindStepById(id);
            return Ok(result);
        }
    }
}
