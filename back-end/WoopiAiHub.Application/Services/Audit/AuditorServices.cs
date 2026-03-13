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

        public AuditorServices(IAuditorRepository auditorRepository)
        {
            _auditorRepository = auditorRepository;
        }

        public Task<ICollection<CardAuditorSummaryDto>> FindCardsAuditSummaryAsync(int take, string? search, int? statusId)
            => _auditorRepository.FindCardsAuditSummaryAsync(take, search, statusId);

        public Task<ICollection<CardAuditorDetailDto>> FindCardAuditDetailsAsync(int cardId, int workflowId, int take, Guid? userId, int? actionType, int? stepId, bool orderDescending = true)
            => _auditorRepository.FindCardAuditDetailsAsync(cardId, workflowId, take, userId, actionType, stepId, orderDescending);

        public Task<ICollection<AuditorWorkflowListItemDto>> FindWorkflowAuditSummaryAsync()
            => _auditorRepository.FindWorkflowAuditSummaryAsync();

        public Task<AuditorWorkflowResponseDto?> FindWorkflowAuditDetailsAsync(int workflowId)
            => _auditorRepository.FindWorkflowAuditDetailsAsync(workflowId);

        public Task<ICollection<UserAuditorSummaryDto>> FindUserAuditSummaryAsync(int skip = 0, string? userName = null, int? teamId = null)
            => _auditorRepository.FindUserAuditSummaryAsync(skip, userName, teamId);

        public Task<UserAuditorDetailsDto?> FindUserAuditDetailsAsync(Guid userId, int? actionTypeCode = null, bool orderDescending = true)
            => _auditorRepository.FindUserAuditDetailsAsync(userId, actionTypeCode, orderDescending);
    }
}
