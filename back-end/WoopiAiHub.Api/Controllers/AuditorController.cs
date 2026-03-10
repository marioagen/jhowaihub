using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.Interfaces.Services.Audit;

namespace WoopiAiHub.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditorController : ControllerBase
    {
        private readonly IAuditorServices _auditorServices;

        public AuditorController(IAuditorServices auditorServices)
        {
            _auditorServices = auditorServices;
        }

        /// <summary>
        /// Returns the first N documents for auditing (load more: take=10 first, then 20, 30...). One row per card.
        /// Optional filters: search (CardId, CardName or WorkflowName), statusId.
        /// </summary>
        [HttpGet("Documents")]
        [SwaggerOperation("Endpoint that returns documents for the auditor (load more pattern)")]
        [ProducesResponseType(typeof(ICollection<AuditorDocumentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocuments(
            [FromQuery] int take = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? statusId = null)
        {
            var result = await _auditorServices.GetDocumentsAsync(take, search, statusId);
            return Ok(result);
        }

        /// <summary>
        /// Returns a single document by id for auditing.
        /// </summary>
        [HttpGet("Document/{id:int}")]
        [SwaggerOperation("Endpoint that returns a document by id for the auditor")]
        [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocument(int id)
        {
            var result = await _auditorServices.GetDocumentByIdAsync(id);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Returns all workflows for auditing. Query params can be used for filtering (managed later).
        /// </summary>
        [HttpGet("Workflows")]
        [SwaggerOperation("Endpoint that returns all workflows for the auditor")]
        [ProducesResponseType(typeof(ICollection<WorkflowDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWorkflows()
        {
            var result = await _auditorServices.GetWorkflowsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns a single workflow by id for auditing.
        /// </summary>
        [HttpGet("Workflow/{id:int}")]
        [SwaggerOperation("Endpoint that returns a workflow by id for the auditor")]
        [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkflow(int id)
        {
            var result = await _auditorServices.GetWorkflowByIdAsync(id);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Returns all users for auditing. Query params can be used for filtering (managed later).
        /// </summary>
        [HttpGet("Users")]
        [SwaggerOperation("Endpoint that returns all users for the auditor")]
        [ProducesResponseType(typeof(ICollection<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _auditorServices.GetUsersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns a single user by id for auditing.
        /// </summary>
        [HttpGet("User/{id:guid}")]
        [SwaggerOperation("Endpoint that returns a user by id for the auditor")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var result = await _auditorServices.GetUserByIdAsync(id);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
    }
}
