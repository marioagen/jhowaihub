using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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

        [HttpGet("Team/{id}")]
        [SwaggerOperation("Endpoint that receive an team id and return a valid workflow")]
        [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindByTeamId(int id)
        {
            var workflow = await _workflowServices.FindByTeamId(id);
            return Ok(workflow);
        }
    }
}
