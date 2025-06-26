using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class DocumentNormalized : BaseEntity
    {
        [Column("Id_Document", TypeName = "int")]
        public int IdDocument { get; private set; }

        [Column("Content", TypeName = "varchar(max)")]
        public string Content { get; private set; } = string.Empty;

        public virtual Document Document { get; set; }

        public DocumentNormalized(int idDocument,
                                  string content,
                                  int id,
                                  DateTime created) : base(id, created)
        {
            IdDocument = idDocument;
            Content = content;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private DocumentNormalized(int id, DateTime created) : base(id, created) { }
    }
}
