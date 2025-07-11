using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class DocumentHistory : BaseEntity
    {
        [Column("Id_Document", TypeName = "int")]
        public int IdDocument { get; private set; }

        [Column("Input", TypeName = "varchar(max)")]
        public string Input { get; private set; } = string.Empty;

        [Column("Output", TypeName = "varchar(max)")]
        public string Output { get; private set; } = string.Empty;

        [Column("IsEdited", TypeName = "bit")]
        public bool IsEdited { get; private set; }

        public virtual Document Document { get; set; }

        public DocumentHistory(int idDocument,
                              string input,
                              string output,
                              int id,
                              DateTime created) : base(id, created)
        {
            IdDocument = idDocument;
            Input = input;
            Output = output;
        }

        /// <summary>
        /// set new output
        /// </summary>
        /// <param name="output"></param>
        public void UpdateOutput(string output)
        {
            Output = output;
            IsEdited = true;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private DocumentHistory(int id, DateTime created) : base(id, created) { }
    }
}
