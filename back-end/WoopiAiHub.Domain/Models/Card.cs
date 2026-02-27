using System.ComponentModel.DataAnnotations.Schema;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models.Audit;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Domain.Models
{
    public class Card : BaseEntity
    {
        [Column("StepId", TypeName = "int")]
        public int StepId { get; private set; }

        [Column("DocumentId", TypeName = "int")]
        public int DocumentId { get; private set; }

        [Column("AssignedUserId", TypeName = "uniqueidentifier")]
        public Guid? AssignedUserId { get; private set; }

        [Column("Name", TypeName = "varchar(255)")]
        public string Name { get; private set; } = string.Empty;

        [Column("StatusId", TypeName = "int")]
        public int StatusId { get; private set; }

        [Column("DocumentBatchId", TypeName = "int")]
        public int? DocumentBatchId { get; private set; }

        public virtual Step? Step { get; set; }
        public virtual Document? Document { get; set; }
        public virtual Status? Status { get; set; }
        public virtual User? AssignedUser { get; set; }
        public virtual DocumentBatch? DocumentBatch { get; set; }
        public virtual ICollection<StepToolExecution> Executions { get; private set; } = new List<StepToolExecution>();
        public virtual ICollection<StepToolOutput> Outputs { get; private set; } = new List<StepToolOutput>();

        public Card(int id, DateTime created, int stepId, int documentId, string name, int statusId, Guid? assignedUserId, int? documentBatchId = null)
            : base(id, created)
        {
            StepId = stepId;
            DocumentId = documentId;
            Name = name;
            StatusId = statusId;
            AssignedUserId = assignedUserId;
            DocumentBatchId = documentBatchId;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Card(int id, DateTime created) : base(id, created) { }

        public void UpdateStepAndStatus(int stepId, int statusId)
        {
            StepId = stepId;
            StatusId = statusId;
        }

        public void UpdateAssignedUser(Guid? userId)
        {
            AssignedUserId = userId;
        }

        public bool IsRejected()
        {
            return StatusId == this.Status?.Id && this.Status.Name == StatusNames.Rejected;
        }

        /// <summary>
        /// Creates an audit log entry for this card via <see cref="AuditCard.Create"/>.
        /// User and timestamp are taken from <paramref name="currentUserService"/> and <see cref="AuditCard.Create"/> (OccurredAt = UTC now) respectively.
        /// </summary>
        /// <param name="workflowId">Id of the workflow the card belongs to.</param>
        /// <param name="actionType">The audit action type.</param>
        /// <param name="currentUserService">Service to resolve the current user; must be authenticated with a valid user Id.</param>
        /// <returns>A new <see cref="AuditCard"/> instance (not yet persisted).</returns>
        /// <exception cref="InvalidOperationException">Thrown when the current user is not authenticated or has no user Id.</exception>
        public AuditCard CreateAuditLog(int workflowId, AuditCardActionType actionType, ICurrentUserService currentUserService)
        {
            return AuditCard.Create(Id, workflowId, actionType, currentUserService);
        }
    }
}
