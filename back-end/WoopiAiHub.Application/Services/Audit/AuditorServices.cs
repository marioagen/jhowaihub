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
        /// Returns a paged list of cards with audit summary (card name, workflows, actions count, status). Supports optional search and status filters.
        /// </summary>
        public Task<ICollection<CardAuditorSummaryDto>> FindCardsAuditSummaryAsync(int take, string? search, int? statusId)
            => _auditorRepository.FindCardsAuditSummaryAsync(take, search, statusId);

        /// <summary>
        /// Returns audit detail rows for a specific card and workflow, with optional filters for user, action type, and step. Supports load-more and sort order.
        /// </summary>
        public Task<ICollection<CardAuditorDetailDto>> FindCardAuditDetailsAsync(int cardId, int workflowId, int take, Guid? userId, int? actionType, int? stepId, bool orderDescending = true)
            => _auditorRepository.FindCardAuditDetailsAsync(cardId, workflowId, take, userId, actionType, stepId, orderDescending);

        /// <summary>
        /// Returns workflow-based audit entries (one per workflow) with card count, logs count, team, and profile. Limited to the 10 most recently audited workflows.
        /// </summary>
        public Task<ICollection<AuditorWorkflowListItemDto>> FindWorkflowAuditSummaryAsync()
            => _auditorRepository.FindWorkflowAuditSummaryAsync();

        /// <summary>
        /// Returns full audit data for a workflow (logs, steps, card status counts, and card list). Returns null when the workflow has no audit entries.
        /// </summary>
        public Task<AuditorWorkflowResponseDto?> FindWorkflowAuditDetailsAsync(int workflowId)
            => _auditorRepository.FindWorkflowAuditDetailsAsync(workflowId);

        /// <summary>
        /// Returns user-based audit summaries (one per user) with teams, profiles, workflow count, and log count. Supports pagination and optional filters by user name and team.
        /// </summary>
        public Task<ICollection<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int skip = 0, string? userName = null, int? teamId = null)
            => _auditorRepository.FindUserAuditSummaryAsync(skip, userName, teamId);

        /// <summary>
        /// Returns full audit details for a user (teams, profiles, action counts, and action list). Returns null when the user has no audit entries. Optional filter by action type and sort order.
        /// </summary>
        public Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, int? actionTypeCode = null, bool orderDescending = true)
            => _auditorRepository.FindUserAuditDetailsAsync(userId, actionTypeCode, orderDescending);
    }
}
