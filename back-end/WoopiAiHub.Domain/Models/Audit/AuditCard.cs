using System.ComponentModel.DataAnnotations.Schema;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Models.Audit
{
    public class AuditCard : BaseEntity
    {
        [Column("CardId", TypeName = "int")]
        public int CardId { get; private set; }

        [Column("WorkflowId", TypeName = "int")]
        public int WorkflowId { get; private set; }

        [Column("DocumentId", TypeName = "int")]
        public int DocumentId { get; private set; }

        [Column("ActionType", TypeName = "int")]
        public AuditCardActionType ActionType { get; private set; }

        [Column("UserId", TypeName = "uniqueidentifier")]
        public Guid UserId { get; private set; }

        public virtual Card? Card { get; set; }
        public virtual Workflow? Workflow { get; set; }
        public virtual User? User { get; set; }
        public virtual Document? Document { get; set; }

        public AuditCard(int id, DateTime created, int cardId, int workflowId, int documentId, AuditCardActionType actionType, Guid userId)
            : base(id, created)
        {
            CardId = cardId;
            WorkflowId = workflowId;
            DocumentId = documentId;
            ActionType = actionType;
            UserId = userId;
        }

        private AuditCard(int id, DateTime created) : base(id, created) { }
    }
}
