using System.ComponentModel.DataAnnotations.Schema;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Models.Audit
{
    public class AuditCard : BaseEntity
    {
        [Column("CardId", TypeName = "int")]
        public int CardId { get; private set; }

        [Column("WorkflowId", TypeName = "int")]
        public int WorkflowId { get; private set; }

        [Column("ActionType", TypeName = "int")]
        public AuditCardActionType ActionType { get; private set; }

        [Column("UserId", TypeName = "uniqueidentifier")]
        public Guid UserId { get; private set; }

        [Column("OccurredAt", TypeName = "datetime")]
        public DateTime OccurredAt { get; private set; }

        public virtual Card? Card { get; set; }
        public virtual Workflow? Workflow { get; set; }
        public virtual User? User { get; set; }

        public AuditCard(int id, DateTime created, int cardId, int workflowId, AuditCardActionType actionType, Guid userId, DateTime occurredAt)
            : base(id, created)
        {
            CardId = cardId;
            WorkflowId = workflowId;
            ActionType = actionType;
            UserId = userId;
            OccurredAt = occurredAt;
        }

        /// <summary>
        /// Use for EF context
        /// </summary>
        private AuditCard(int id, DateTime created) : base(id, created) { }

        /// <summary>
        /// Creates a new AuditCard for a card action. Id and Created are set to 0 and current UTC time respectively; the database will assign the actual Id on save.
        /// User and OccurredAt are taken from <paramref name="currentUserService"/> and UTC now.
        /// </summary>
        /// <param name="cardId">Id of the card.</param>
        /// <param name="workflowId">Id of the workflow.</param>
        /// <param name="actionType">The audit action type.</param>
        /// <param name="currentUserService">Service to resolve the current user; must be authenticated with a valid user Id.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="actionType"/> is not a defined value of <see cref="AuditCardActionType"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user is not authenticated or has no user Id.</exception>
        public static AuditCard Create(int cardId, int workflowId, AuditCardActionType actionType, ICurrentUserService currentUserService)
        {
            if (!Enum.IsDefined(typeof(AuditCardActionType), actionType))
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, $"Action type must be a defined value of {nameof(AuditCardActionType)}.");

            if (!currentUserService.IsAuthenticated || currentUserService.Id is not { } userId)
                throw new InvalidOperationException("Current user is required to create an audit log.");

            var at = DateTime.UtcNow;
            return new AuditCard(0, at, cardId, workflowId, actionType, userId, at);
        }
    }
}
