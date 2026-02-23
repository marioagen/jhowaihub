using System.ComponentModel.DataAnnotations.Schema;
using WoopiAiHub.Domain.Enum;
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

        public virtual Step? Step { get; set; }
        public virtual Document? Document { get; set; }
        public virtual Status? Status { get; set; }
        public virtual User? AssignedUser { get; set; }
        public virtual ICollection<StepToolExecution> Executions { get; private set; } = new List<StepToolExecution>();
        public virtual ICollection<StepToolOutput> Outputs { get; private set; } = new List<StepToolOutput>();

        public Card(int id, DateTime created, int stepId, int documentId, string name, int statusId, Guid? assignedUserId)
            : base(id, created)
        {
            StepId = stepId;
            DocumentId = documentId;
            Name = name;
            StatusId = statusId;
            AssignedUserId = assignedUserId;
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
    }
}
