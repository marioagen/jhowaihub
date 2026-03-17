using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.DTOs.Response.Auditor;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Services.Audit;

namespace WoopiAiHub.Application.Services.Audit
{
    /// <summary>
    /// Service for auditor endpoints. Delegates to auditor repository.
    /// </summary>
    public class AuditorServices : IAuditorServices
    {
        private readonly IAuditorRepository _auditorRepository;

        /// <summary>
        /// Initializes the auditor service with the auditor repository.
        /// </summary>
        public AuditorServices(IAuditorRepository auditorRepository)
        {
            _auditorRepository = auditorRepository;
        }

        /// <summary>
        /// Returns a paged list of documents with audit summary (DocumentId, DocumentName, Workflows with DocumentId, ActionsCount, IsFinalized). Supports optional search and status filter.
        /// </summary>
        public Task<ICollection<CardAuditorSummaryDto>> FindCardsAuditSummaryAsync(int take, string? search, bool? isFinalized = null)
            => _auditorRepository.FindCardsAuditSummaryAsync(take, search, isFinalized);

        /// <summary>
        /// Returns document audit detail for a document and workflow (DocumentId, DocumentName, WorkflowId, WorkflowName, DocumentHistory), with optional filters. Returns null when no audit rows exist.
        /// </summary>
        public Task<CardAuditorDetailDto?> FindCardAuditDetailsAsync(int documentId, int workflowId, int take, string? search = null, Guid? userId = null, int? actionType = null, int? stepId = null, bool orderDescending = true)
            => _auditorRepository.FindCardAuditDetailsAsync(documentId, workflowId, take, search, userId, actionType, stepId, orderDescending);

        /// <summary>
        /// Returns workflow-based audit entries (one per workflow) with document count, logs count, team, and profile. Load-more pattern: take 10, 20, 30, … Optional search by workflow or team name.
        /// </summary>
        public Task<ICollection<WorkflowAuditorSummaryDto>> FindWorkflowAuditSummaryAsync(int take = 10, string? search = null)
            => _auditorRepository.FindWorkflowAuditSummaryAsync(take, search);

        /// <summary>
        /// Returns full audit data for a workflow (LogCount, StepsCount with DocumentCount per step, document-level status counts, and Cards audit history). Returns null when the workflow has no audit entries. Optional filters: search, stepId, actionType.
        /// </summary>
        public Task<WorkflowAuditorDetailsDto?> FindWorkflowAuditDetailsAsync(int workflowId, string? search = null, int? stepId = null, int? actionType = null, bool orderDescending = true)
            => _auditorRepository.FindWorkflowAuditDetailsAsync(workflowId, search, stepId, actionType, orderDescending);

        /// <summary>
        /// Returns user-based audit summaries (one per user) with teams, profiles, workflow count, and log count. Load-more pattern: take 10, 20, 30, … Optional filters by user name and team.
        /// </summary>
        public Task<ICollection<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int take = 10, string? userName = null, int? teamId = null)
            => _auditorRepository.FindUserAuditSummaryAsync(take, userName, teamId);

        /// <summary>
        /// Returns full audit details for a user (teams, profiles, action counts, and action list). Returns null when the user has no audit entries. Optional filter by search, action type, and sort order.
        /// </summary>
        public Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, string? search = null, int? actionTypeCode = null, bool orderDescending = true)
            => _auditorRepository.FindUserAuditDetailsAsync(userId, search, actionTypeCode, orderDescending);
    }
}
