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
        /// Returns the first N documents for the auditor (load-more pattern). One row per document with DocumentId, DocumentName, Workflows (with DocumentId), ActionsCount, IsFinalized (from DB).
        /// </summary>
        /// <param name="take">Maximum number of documents to return (default 10).</param>
        /// <param name="search">Optional. Matches DocumentId/CardId when numeric, or DocumentName/CardName/WorkflowName by contains.</param>
        /// <param name="isFinalized">Optional. When true, only finalized documents (all cards finalized); when false, only non-finalized; when null, all.</param>
        [HttpGet("Cards")]
        [SwaggerOperation("Returns cards for the auditor with optional search and status filter")]
        [ProducesResponseType(typeof(ICollection<CardAuditorSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindCardsAuditSummary(
            [FromQuery] int take = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isFinalized = null)
        {
            var result = await _auditorServices.FindCardsAuditSummaryAsync(take, search, isFinalized);
            return Ok(result);
        }

        /// <summary>
        /// Returns document audit detail for the given document and workflow: DocumentId, DocumentName, WorkflowId, WorkflowName, and DocumentHistory (up to N audit entries). Load-more pattern.
        /// </summary>
        /// <param name="documentId">Document identifier.</param>
        /// <param name="workflowId">Workflow identifier.</param>
        /// <param name="take">Maximum number of audit entries to return in DocumentHistory (default 10).</param>
        /// <param name="search">Optional. Matches UserName, DocumentName, CardName, ActionType, or StepName by contains.</param>
        /// <param name="userId">Optional. Filter by user who performed the action.</param>
        /// <param name="action">Optional. Filter by action type (AuditCardActionType enum value).</param>
        /// <param name="step">Optional. Filter by step id.</param>
        /// <param name="orderDescending">Order by Created descending when true (default), ascending when false.</param>
        [HttpGet("Documents/{documentId:int}/Workflows/{workflowId:int}")]
        [SwaggerOperation("Returns document audit detail for a document and workflow with optional filters and sort")]
        [ProducesResponseType(typeof(CardAuditorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindCardAuditDetails(
            int documentId,
            int workflowId,
            [FromQuery] int take = 10,
            [FromQuery] string? search = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] int? action = null,
            [FromQuery] int? step = null,
            [FromQuery] bool orderDescending = true)
        {
            var result = await _auditorServices.FindCardAuditDetailsAsync(documentId, workflowId, take, search, userId, action, step, orderDescending);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Returns workflow-based audit entries (one row per workflow) with DocumentCount, LogsCount, Team, Profile. Load-more pattern: take 10, 20, 30, …
        /// </summary>
        /// <param name="take">Maximum number of workflows to return (default 10).</param>
        /// <param name="search">Optional. Matches WorkflowName or TeamName by contains.</param>
        [HttpGet("Workflows")]
        [SwaggerOperation("Returns workflow audit list for the auditor with optional search")]
        [ProducesResponseType(typeof(ICollection<WorkflowAuditorSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindWorkflowAuditSummary([FromQuery] int take = 10, [FromQuery] string? search = null)
        {
            var result = await _auditorServices.FindWorkflowAuditSummaryAsync(take, search);
            return Ok(result);
        }

        /// <summary>
        /// Returns audit data for a workflow by id: WorkflowId, WorkflowName, LogCount, StepsCount (with DocumentCount per step), document-level status counts (TotalDocuments, Finalized, Rejected), and Cards (audit history). Returns 404 when no audit entries exist for the workflow.
        /// </summary>
        /// <param name="id">Workflow id.</param>
        /// <param name="search">Optional. Matches UserName, CardName, StepName, or ActionType by contains.</param>
        /// <param name="stepId">Optional. Filter by step id.</param>
        /// <param name="actionType">Optional. Filter by action type (AuditCardActionType enum value as int).</param>
        /// <param name="orderDescending">Order cards by Created descending when true (default), ascending when false.</param>
        [HttpGet("Workflow/{id:int}")]
        [SwaggerOperation("Returns audit data for a workflow by id with optional filters")]
        [ProducesResponseType(typeof(WorkflowAuditorDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindWorkflowAuditDetails(
            int id,
            [FromQuery] string? search = null,
            [FromQuery] int? stepId = null,
            [FromQuery] int? actionType = null,
            [FromQuery] bool orderDescending = true)
        {
            var result = await _auditorServices.FindWorkflowAuditDetailsAsync(id, search, stepId, actionType, orderDescending);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Returns user-based audit entries (one row per user) with UserId, UserName, Teams, Profiles, WorkflowCount, LogCount. Load-more pattern: take 10, 20, 30, …
        /// </summary>
        /// <param name="take">Maximum number of users to return (default 10).</param>
        /// <param name="userName">Optional. Filter by user name (contains, case-sensitive).</param>
        /// <param name="teamId">Optional. Filter to users that have at least one audit entry in a workflow with this team.</param>
        [HttpGet("Users")]
        [SwaggerOperation("Returns user audit list for the auditor with load-more and filters")]
        [ProducesResponseType(typeof(ICollection<UserAuditorSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindUserAuditSummary([FromQuery] int take = 10, [FromQuery] string? userName = null, [FromQuery] int? teamId = null)
        {
            var result = await _auditorServices.FindUserAuditSummaryAsync(take, userName, teamId);
            return Ok(result);
        }

        /// <summary>
        /// Returns full audit details for a user: UserId, UserName, Teams, Profiles, log counts (total and by action type), and list of actions. Returns 404 when the user has no audit entries.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="search">Optional. Matches CardName, WorkflowName, or ActionType by contains.</param>
        /// <param name="actionTypeCode">Optional. Filter by action type (AuditCardActionType enum value as int).</param>
        /// <param name="orderDescending">Order by Created: true = newest first (default), false = oldest first.</param>
        [HttpGet("User/{userId:guid}")]
        [SwaggerOperation("Returns user audit details by user id with optional filters and sort")]
        [ProducesResponseType(typeof(UserAuditorDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindUserAuditDetails(Guid userId, [FromQuery] string? search = null, [FromQuery] int? actionTypeCode = null, [FromQuery] bool orderDescending = true)
        {
            var result = await _auditorServices.FindUserAuditDetailsAsync(userId, search, actionTypeCode, orderDescending);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
    }
}
