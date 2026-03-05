using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models.Audit;

namespace WoopiAiHub.Domain.Interfaces.Repository.Audit
{
    public interface IAuditCardRepository
    {
        Task AddAsync(AuditCard auditCard, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<AuditCard> auditCards, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a single audit card entry with validation and persists it.
        /// </summary>
        Task CreateAndSaveAsync(int cardId, int workflowId, AuditCardActionType actionType, ICurrentUserService currentUserService, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates audit card entries for the given card/workflow pairs with validation and persists them.
        /// </summary>
        Task CreateBatchAndSaveAsync(IReadOnlyList<(int cardId, int workflowId)> cardWorkflows, AuditCardActionType actionType, ICurrentUserService currentUserService, CancellationToken cancellationToken = default);
    }
}
