using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Interfaces.Repository.Audit;
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

        [Column("Enable", TypeName = "bit")]
        public bool Enable { get; private set; } = true;

        public virtual Step? Step { get; set; }
        public virtual Document? Document { get; set; }
        public virtual Status? Status { get; set; }
        public virtual User? AssignedUser { get; set; }
        public virtual DocumentBatch? DocumentBatch { get; set; }
        public virtual ICollection<StepToolExecution> Executions { get; private set; } = new List<StepToolExecution>();
        public virtual ICollection<StepToolOutput> Outputs { get; private set; } = new List<StepToolOutput>();

        public Card(int id, DateTime created, int stepId, int documentId, string name, int statusId, Guid? assignedUserId, int? documentBatchId = null, bool enable = true)
            : base(id, created)
        {
            StepId = stepId;
            DocumentId = documentId;
            Name = name;
            StatusId = statusId;
            AssignedUserId = assignedUserId;
            DocumentBatchId = documentBatchId;
            Enable = enable;
        }

        private Card(int id, DateTime created) : base(id, created) { }

        public void UpdateStepAndStatus(int stepId, int statusId)
        {
            StepId = stepId;
            StatusId = statusId;
        }

        public static void UpdateStepAndStatus(IEnumerable<Card> cards, int stepId, int statusId)
        {
            foreach (var card in cards)
                card.UpdateStepAndStatus(stepId, statusId);
        }

        public static void UpdateStepAndStatus(IEnumerable<Card> cards, int stepId, Func<Card, int> getStatusId)
        {
            foreach (var card in cards)
                card.UpdateStepAndStatus(stepId, getStatusId(card));
        }

        public void UpdateAssignedUser(Guid? userId)
        {
            AssignedUserId = userId;
        }

        public static void UpdateAssignedUser(IEnumerable<Card> cards, Guid? userId)
        {
            foreach (var card in cards)
                card.UpdateAssignedUser(userId);
        }

        public void Disable()
        {
            Enable = false;
        }

        public bool IsRejected()
        {
            return StatusId == this.Status?.Id && this.Status.Name == StatusNames.Rejected;
        }

        public void CreateAuditLog(int workflowId, AuditCardActionType actionType, ICurrentUserService currentUserService, IAuditCardRepository auditCardRepository)
        {
            AuditCard.Create(Id, workflowId, actionType, currentUserService, auditCardRepository);
        }
    }
}
