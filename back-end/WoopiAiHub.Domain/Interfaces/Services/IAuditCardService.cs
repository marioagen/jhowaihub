using WoopiAiHub.Domain.Enum.Audit;

namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IAuditCardService
    {
        /// <summary>
        /// Creates a single audit card entry with validation and persists it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="actionType"/> is not a defined value of <see cref="AuditCardActionType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user is not authenticated or has no user Id.</exception>
        Task CreateAndSaveAsync(int cardId, int workflowId, AuditCardActionType actionType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates audit card entries for the given card/workflow pairs with validation and persists them.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="actionType"/> is not a defined value of <see cref="AuditCardActionType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user is not authenticated or has no user Id.</exception>
        Task CreateBatchAndSaveAsync(IReadOnlyList<(int cardId, int workflowId)> cardWorkflows, AuditCardActionType actionType, CancellationToken cancellationToken = default);
    }
}
