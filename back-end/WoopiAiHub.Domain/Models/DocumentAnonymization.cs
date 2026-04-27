using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class DocumentAnonymization : BaseEntity
    {
        [Column("DocumentId", TypeName = "int")]
        public int DocumentId { get; private set; }

        [Column("DocumentUrl", TypeName = "nvarchar(max)")]
        public string DocumentUrl { get; private set; } = string.Empty;

        public virtual Document? Document { get; set; }

        public DocumentAnonymization(int id, DateTime created, int documentId, string documentUrl)
            : base(id, created)
        {
            DocumentId = documentId;
            DocumentUrl = documentUrl;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private DocumentAnonymization(int id, DateTime created) : base(id, created) { }
    }
}
