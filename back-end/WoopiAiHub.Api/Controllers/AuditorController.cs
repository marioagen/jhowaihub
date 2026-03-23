using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.DTOs.Response.Auditor.Documents;
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
        /// Returns all audit card action types as code and name for use in filters and dropdowns.
        /// </summary>
        [HttpGet("ActionTypes")]
        [SwaggerOperation("Returns audit action type enum values as code and name")]
        [ProducesResponseType(typeof(ICollection<AuditorActionTypeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindActionTypes()
        {
            var result = await _auditorServices.FindActionTypesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns documents for the auditor: one row per document with DocumentId, DocumentName, Workflows (with DocumentId), ActionsCount, IsFinalized. LoadMore logic: take (page size) and skip (offset); backend returns items and hasMore.
        /// </summary>
        /// <param name="take">Number of documents to return (default 10).</param>
        /// <param name="skip">Number of documents to skip (default 0).</param>
        /// <param name="search">Optional. Matches DocumentId when numeric, or DocumentName/WorkflowName by contains.</param>
        /// <param name="isFinalized">Optional. When true, only finalized documents; when false, only non-finalized; when null, all (unless combined with IsRemoved).</param>
        /// <param name="isRemoved">Optional. When true, only soft-deleted documents; when false, only non-deleted; when null, no filter by document enabled state.</param>
        [HttpGet("Documents")]
        [SwaggerOperation("Returns documents for the auditor with optional search and status filter")]
        [ProducesResponseType(typeof(AuditorLoadMoreResultDto<DocumentAuditorSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindDocumentsAuditSummary(
            [FromQuery] int take = 10,
            [FromQuery] int skip = 0,
            [FromQuery] string? search = null,
            [FromQuery] bool? isFinalized = null,
            [FromQuery] bool? isRemoved = null)
        {
            var result = await _auditorServices.FindDocumentsAuditSummaryAsync(take, skip, search, isFinalized, isRemoved);
            return Ok(result);
        }

        /// <summary>
        /// Returns document audit detail for the given document and workflow: DocumentId, DocumentName, WorkflowId, WorkflowName, and DocumentHistory (up to N audit entries). Load-more pattern.
        /// </summary>
        /// <param name="documentId">Document identifier.</param>
        /// <param name="workflowId">Workflow identifier.</param>
        /// <param name="take">Maximum number of audit entries to return in DocumentHistory (default 10).</param>
        /// <param name="search">Optional. Matches UserName, DocumentName, ActionType, or StepName by contains.</param>
        /// <param name="userId">Optional. Filter by user who performed the action.</param>
        /// <param name="action">Optional. Filter by action type (AuditCardActionType enum value).</param>
        /// <param name="step">Optional. Filter by step id.</param>
        /// <param name="orderDescending">Order by Created descending when true (default), ascending when false.</param>
        [HttpGet("Documents/{documentId:int}/Workflows/{workflowId:int}")]
        [SwaggerOperation("Returns document audit detail for a document and workflow with optional filters and sort")]
        [ProducesResponseType(typeof(DocumentAuditorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindDocumentAuditDetails(
            int documentId,
            int workflowId,
            [FromQuery] int take = 10,
            [FromQuery] string? search = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] int? action = null,
            [FromQuery] int? step = null,
            [FromQuery] bool orderDescending = true)
        {
            var result = await _auditorServices.FindDocumentAuditDetailsAsync(documentId, workflowId, take, search, userId, action, step, orderDescending);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Returns workflow-based audit entries (one row per workflow) with DocumentCount, LogsCount, Team, Profile. LoadMore logic: take (page size) and skip (offset); backend returns items and hasMore.
        /// </summary>
        /// <param name="take">Number of workflows to return (default 10).</param>
        /// <param name="skip">Number of workflows to skip (default 0).</param>
        /// <param name="search">Optional. Matches WorkflowName or TeamName by contains.</param>
        [HttpGet("Workflows")]
        [SwaggerOperation("Returns workflow audit list for the auditor with optional search")]
        [ProducesResponseType(typeof(AuditorLoadMoreResultDto<WorkflowAuditorSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindWorkflowAuditSummary([FromQuery] int take = 10, [FromQuery] int skip = 0, [FromQuery] string? search = null)
        {
            var result = await _auditorServices.FindWorkflowAuditSummaryAsync(take, skip, search);
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
        /// <param name="take">Maximum number of timeline cards to return (default 10). Load more: 10, 20, 30, …</param>
        [HttpGet("Workflow/{id:int}")]
        [SwaggerOperation("Returns audit data for a workflow by id with optional filters")]
        [ProducesResponseType(typeof(WorkflowAuditorDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindWorkflowAuditDetails(
            int id,
            [FromQuery] string? search = null,
            [FromQuery] int? stepId = null,
            [FromQuery] int? actionType = null,
            [FromQuery] bool orderDescending = true,
            [FromQuery] int take = 10)
        {
            var result = await _auditorServices.FindWorkflowAuditDetailsAsync(id, take, search, stepId, actionType, orderDescending);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Returns user-based audit entries (one row per user) with UserId, UserName, Teams, Profiles, WorkflowCount, LogCount. Pagination: take (page size) and skip (offset); backend returns items and hasMore.
        /// </summary>
        /// <param name="take">Number of users to return (default 10).</param>
        /// <param name="skip">Number of users to skip (default 0).</param>
        /// <param name="userName">Optional. Filter by user name (contains, case-sensitive).</param>
        /// <param name="teamId">Optional. Filter to users that have at least one audit entry in a workflow with this team.</param>
        [HttpGet("Users")]
        [SwaggerOperation("Returns user audit list for the auditor with load-more and filters")]
        [ProducesResponseType(typeof(AuditorLoadMoreResultDto<UserAuditorSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindUserAuditSummary([FromQuery] int take = 10, [FromQuery] int skip = 0, [FromQuery] string? userName = null, [FromQuery] int? teamId = null)
        {
            var result = await _auditorServices.FindUserAuditSummaryAsync(take, skip, userName, teamId);
            return Ok(result);
        }

        /// <summary>
        /// Returns full audit details for a user: UserId, UserName, Teams, Profiles, log counts (total and by action type), and list of actions. Returns 404 when the user has no audit entries.
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="search">Optional. Matches CardName, WorkflowName, or ActionType by contains.</param>
        /// <param name="actionTypeCode">Optional. Filter by action type (AuditCardActionType enum value as int).</param>
        /// <param name="orderDescending">Order by Created: true = newest first (default), false = oldest first.</param>
        /// <param name="take">Maximum number of activity entries to return (default 10). Load more: 10, 20, 30, …</param>
        [HttpGet("User/{userId:guid}")]
        [SwaggerOperation("Returns user audit details by user id with optional filters and sort")]
        [ProducesResponseType(typeof(UserAuditorDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FindUserAuditDetails(Guid userId, [FromQuery] string? search = null, [FromQuery] int? actionTypeCode = null, [FromQuery] bool orderDescending = true, [FromQuery] int take = 10)
        {
            var result = await _auditorServices.FindUserAuditDetailsAsync(userId, take, search, actionTypeCode, orderDescending);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
    }
}
