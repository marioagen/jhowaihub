using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Api.Attributes;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Api.Controllers.Internal
{
    [KeyExternalAccessAuthorization]
    [Route("api/internal/[controller]")]
    [ApiController]
    public class WorkflowController(IWorkflowServices workflowServices) : ControllerBase
    {
        private readonly IWorkflowServices _workflowServices = workflowServices;

        /// <summary>
        /// Returns all workflows in a basic format, which includes only the essential information needed for internal use. This endpoint is intended for internal use only and should not be exposed to external clients.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("Endpoint that receives the request to return all workflow in basic format")]
        [ProducesResponseType(typeof(ICollection<WorkflowInternalDto>), StatusCodes.Status200OK)]
        public IActionResult FindAllInternal()
        {
            var result = _workflowServices.FindAllInternal();
            return Ok(result);
        }
    }
}
