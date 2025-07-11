using WoopiAiHub.Domain.Enum;
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
        public Status Status { get; private set; }

        [Column("Enable", TypeName = "bit")]
        public bool Enable { get; private set; }

        [Column("EmailCreator", TypeName = "varchar(50)")]
        public string EmailCreator { get; private set; } = string.Empty;

        public virtual ICollection<DocumentHistory> DocumentHistories { get; set; }
        public virtual DocumentNormalized? DocumentNormalized { get; set; }

        public Document(string name,
                       string description,
                       string referenceFile,
                       Status status,
                       bool enable,
                       string emailCreator,
                       int id,
                       DateTime created) : base(id, created)
        {
            this.Name = name;
            this.Description = description;
            this.ReferenceFile = referenceFile;
            this.Status = status;
            this.Enable = enable;
            this.EmailCreator = emailCreator;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Document(int id, DateTime created) : base(id, created) { }
    }
}
