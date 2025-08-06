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
        public string Name { get; private set; }

        [Column("StatusId", TypeName = "int")]
        public int StatusId { get; private set; }

        public virtual Step Step { get; private set; }
        public virtual Document Document { get; private set; }
        public virtual StepStatus Status { get; private set; }

        public Card(int id, DateTime created, int stepId, int documentId, string name, int statusId)
            : base(id, created)
        {
            StepId = stepId;
            DocumentId = documentId;
            Name = name;
            StatusId = statusId;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Card(int id, DateTime created) : base(id, created) { }
    }
}
