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
        /// Returns the first N cards for the auditor (load-more pattern). One row per card with CardId, CardName, Workflows, ActionsCount, StatusName.
        /// </summary>
        /// <param name="take">Maximum number of cards to return (default 10).</param>
        /// <param name="search">Optional. Matches CardId when numeric, or CardName/WorkflowName by contains.</param>
        /// <param name="statusId">Optional. Exact match on card status.</param>
        [HttpGet("Cards")]
        [SwaggerOperation("Returns cards for the auditor with optional search and status filter")]
        [ProducesResponseType(typeof(ICollection<CardAuditorSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindCardsAuditSummary(
            [FromQuery] int take = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? statusId = null)
        {
            var result = await _auditorServices.FindCardsAuditSummaryAsync(take, search, statusId);
            return Ok(result);
        }

        /// <summary>
        /// Returns up to N audit rows for the given card and workflow (load-more pattern).
        /// </summary>
        /// <param name="cardId">Card identifier.</param>
        /// <param name="workflowId">Workflow identifier.</param>
        /// <param name="take">Maximum number of audit rows to return (default 10).</param>
        /// <param name="userId">Optional. Filter by user who performed the action.</param>
        /// <param name="action">Optional. Filter by action type (AuditCardActionType enum value).</param>
        /// <param name="step">Optional. Filter by step id.</param>
        /// <param name="orderDescending">Order by Created descending when true (default), ascending when false.</param>
        [HttpGet("Cards/{cardId:int}/Workflows/{workflowId:int}")]
        [SwaggerOperation("Returns audit rows for a card and workflow with optional filters and sort")]
        [ProducesResponseType(typeof(ICollection<CardAuditorDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindCardAuditDetails(
            int cardId,
            int workflowId,
            [FromQuery] int take = 10,
            [FromQuery] Guid? userId = null,
            [FromQuery] int? action = null,
            [FromQuery] int? step = null,
            [FromQuery] bool orderDescending = true)
        {
            var result = await _auditorServices.FindCardAuditDetailsAsync(cardId, workflowId, take, userId, action, step, orderDescending);
            return Ok(result);
        }

        /// <summary>
        /// Returns workflow-based audit entries (one row per workflow) with CardCount, CardsByStatus, LogsCount, Team, Profile. Limited to 10 entries. Filters and take to be added later.
        /// </summary>
        [HttpGet("Workflows")]
        [SwaggerOperation("Returns workflow audit list for the auditor")]
        [ProducesResponseType(typeof(ICollection<AuditorWorkflowListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindWorkflowAuditSummary()
        {
            var result = await _auditorServices.FindWorkflowAuditSummaryAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns audit data for a workflow by id: WorkflowId, WorkflowName, LogCount, StepsCount, CardStatusCount, Cards. Returns 404 when no audit entries exist for the workflow.
        /// </summary>
        [HttpGet("Workflow/{id:int}")]
        [SwaggerOperation("Returns audit data for a workflow by id")]
        [ProducesResponseType(typeof(AuditorWorkflowResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindWorkflowAuditDetails(int id)
        {
            var result = await _auditorServices.FindWorkflowAuditDetailsAsync(id);
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
        public async Task<IActionResult> FindUserAuditSummary()
        {
            var result = await _auditorServices.FindUserAuditSummaryAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns a single user by id for auditing.
        /// </summary>
        [HttpGet("User/{id:guid}")]
        [SwaggerOperation("Endpoint that returns a user by id for the auditor")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindUserAuditDetails(Guid id)
        {
            var result = await _auditorServices.FindUserAuditDetailsAsync(id);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
    }
}
