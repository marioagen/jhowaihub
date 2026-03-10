using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models.Audit;

namespace WoopiAiHub.Application.Services.Audit
{
    public class AuditCardService : IAuditCardService
    {
        private readonly IAuditCardRepository _auditCardRepository;
        private readonly ICurrentUserService _currentUserService;

        public AuditCardService(IAuditCardRepository auditCardRepository, ICurrentUserService currentUserService)
        {
            _auditCardRepository = auditCardRepository;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Creates a single audit card entry with validation and persists it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="actionType"/> is not a defined value of <see cref="AuditCardActionType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user is not authenticated or has no user Id.</exception>
        public async Task CreateAndSaveAsync(int cardId, int workflowId, AuditCardActionType actionType, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(AuditCardActionType), actionType))
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, $"Action type must be a defined value of {nameof(AuditCardActionType)}.");

            if (!_currentUserService.IsAuthenticated || _currentUserService.Id is not { } userId)
                throw new InvalidOperationException("Current user is required to create an audit log.");

            var auditCard = new AuditCard(0, DateTime.UtcNow, cardId, workflowId, actionType, userId);
            await _auditCardRepository.AddAsync(auditCard, cancellationToken);
        }

        /// <summary>
        /// Creates audit card entries for the given card/workflow pairs with validation and persists them.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="actionType"/> is not a defined value of <see cref="AuditCardActionType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user is not authenticated or has no user Id.</exception>
        public async Task CreateBatchAndSaveAsync(IReadOnlyList<(int cardId, int workflowId)> cardWorkflows, AuditCardActionType actionType, CancellationToken cancellationToken = default)
        {
            if (cardWorkflows.Count == 0)
                return;

            if (!Enum.IsDefined(typeof(AuditCardActionType), actionType))
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, $"Action type must be a defined value of {nameof(AuditCardActionType)}.");

            if (!_currentUserService.IsAuthenticated || _currentUserService.Id is not { } userId)
                throw new InvalidOperationException("Current user is required to create an audit log.");

            var now = DateTime.UtcNow;
            var auditCards = new List<AuditCard>(cardWorkflows.Count);
            foreach (var (cardId, workflowId) in cardWorkflows)
                auditCards.Add(new AuditCard(0, now, cardId, workflowId, actionType, userId));

            await _auditCardRepository.AddRangeAsync(auditCards, cancellationToken);
        }
    }
}
