using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Document : BaseEntity
    {
        [Column("Name", TypeName = "varchar(251)")]
        public string Name { get; private set; } = string.Empty;

        [Column("Description", TypeName = "varchar(100)")]
        public string Description { get; private set; } = string.Empty;

        [Column("Reference_File", TypeName = "varchar(50)")]
        public string ReferenceFile { get; private set; } = string.Empty;

        [Column("Status", TypeName = "int")]
        public Enum.DocumentStatus Status { get; private set; }

        [Column("EmailCreator", TypeName = "varchar(50)")]
        public string EmailCreator { get; private set; } = string.Empty;

        [Column("HasBatch", TypeName = "bit")]
        public bool HasBatch { get; private set; } = false;

        [Column("Enable", TypeName = "bit")]
        public bool Enable { get; private set; } = true;

        public virtual ICollection<DocumentHistory> DocumentHistories { get; set; }
        public virtual DocumentNormalized? DocumentNormalized { get; set; }
        public virtual ICollection<Card> Cards { get; set; }
        public virtual ICollection<Workflow> Workflows { get; set; }
        public virtual ICollection<DocumentAnonymization> DocumentAnonymizations { get; set; }

        public Document(string name,
                       string description,
                       string referenceFile,
                       Enum.DocumentStatus status,
                       string emailCreator,
                       int id,
                       List<Workflow> workflow,
                       DateTime created,
                       bool hasBatch = false) : base(id, created)
        {
            Name = name;
            Description = description;
            ReferenceFile = referenceFile;
            Status = status;
            Workflows = workflow;
            EmailCreator = emailCreator;
            HasBatch = hasBatch;
        }

        public void Disable()
        {
            Enable = false;
        }

        private Document(int id, DateTime created) : base(id, created) { }
    }
}
