using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository;
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
        private readonly IUserRepository _userRepository;

        public AuditCardService(IAuditCardRepository auditCardRepository, ICurrentUserService currentUserService, IUserRepository userRepository)
        {
            _auditCardRepository = auditCardRepository;
            _currentUserService = currentUserService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Creates a single audit card entry with validation and persists it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="actionType"/> is not a defined value of <see cref="AuditCardActionType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user is not authenticated or has no user Id, and no valid <paramref name="automationUserEmail"/> is provided.</exception>
        public async Task CreateAndSaveAsync(int cardId, int workflowId, AuditCardActionType actionType, string? automationUserEmail = null, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(AuditCardActionType), actionType))
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, $"Action type must be a defined value of {nameof(AuditCardActionType)}.");

            var userId = await ResolveUserIdAsync(automationUserEmail);

            var auditCard = new AuditCard(0, DateTime.UtcNow, cardId, workflowId, actionType, userId);
            await _auditCardRepository.AddAsync(auditCard, cancellationToken);
        }

        /// <summary>
        /// Creates audit card entries for the given card/workflow pairs with validation and persists them.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="actionType"/> is not a defined value of <see cref="AuditCardActionType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user is not authenticated or has no user Id, and no valid <paramref name="automationUserEmail"/> is provided.</exception>
        public async Task CreateBatchAndSaveAsync(IReadOnlyList<(int cardId, int workflowId)> cardWorkflows, AuditCardActionType actionType, string? automationUserEmail = null, CancellationToken cancellationToken = default)
        {
            if (cardWorkflows.Count == 0)
                return;

            if (!Enum.IsDefined(typeof(AuditCardActionType), actionType))
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, $"Action type must be a defined value of {nameof(AuditCardActionType)}.");

            var userId = await ResolveUserIdAsync(automationUserEmail);

            var now = DateTime.UtcNow;
            var auditCards = new List<AuditCard>(cardWorkflows.Count);
            foreach (var (cardId, workflowId) in cardWorkflows)
                auditCards.Add(new AuditCard(0, now, cardId, workflowId, actionType, userId));

            await _auditCardRepository.AddRangeAsync(auditCards, cancellationToken);
        }

        private async Task<Guid> ResolveUserIdAsync(string? automationUserEmail)
        {
            if (_currentUserService.IsAuthenticated && _currentUserService.Id is { } userId)
                return userId;

            if (!string.IsNullOrWhiteSpace(automationUserEmail))
            {
                var user = await _userRepository.FindByEmailAsync(automationUserEmail);
                if (user != null)
                {
                    return user.Id;
                }
            }

            throw new InvalidOperationException("Current user is required to create an audit log. When running in automation context, provide the user email from the automation DTO.");
        }
    }
}
