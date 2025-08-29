using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Card : BaseEntity
    {
        [Column("StepId", TypeName = "int")]
        public int StepId { get; private set; }

        [Column("DocumentId", TypeName = "int")]
        public int DocumentId { get; private set; }

        [Column("Name", TypeName = "varchar(255)")]
        public string Name { get; private set; } = string.Empty;

        [Column("StatusId", TypeName = "int")]
        public int StatusId { get; private set; }

        [Column("Enable", TypeName = "bit")]
        public bool Enable { get; private set; }

        public virtual Step? Step { get; set; }
        public virtual Document? Document { get; set; }
        public virtual Status? Status { get; set; }

        public Card(int id, DateTime created, int stepId, int documentId, string name, int statusId, bool enable)
            : base(id, created)
        {
            StepId = stepId;
            DocumentId = documentId;
            Name = name;
            StatusId = statusId;
            Enable = enable;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Card(int id, DateTime created) : base(id, created) { }

        public void UpdateStepAndSatus(int stepId, int statusId)
        {
            StepId = stepId;
            StatusId = statusId;
        }
    }
}
