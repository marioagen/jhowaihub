using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class DocumentAnalysisRejection : BaseEntity
    {
        [Column("Justification", TypeName = "nvarchar(MAX)")]
        public string Justification { get; private set; } = string.Empty;

        [Column("CardId", TypeName = "int")]
        public int CardId { get; private set; }

        [Column("StepId", TypeName = "int")]
        public int StepId { get; private set; }

        [Column("UserId", TypeName = "uniqueidentifier")]
        public Guid UserId { get; private set; }

        public virtual Card? Card { get; set; }
        public virtual Step? Step { get; set; }
        public virtual User? User { get; set; }

        public DocumentAnalysisRejection(int id, DateTime created, string justification, int cardId, int stepId, Guid userId)
            : base(id, created)
        {
            Justification = justification;
            CardId = cardId;
            StepId = stepId;
            UserId = userId;
        }

        private DocumentAnalysisRejection(int id, DateTime created) : base(id, created) { }
    }
}
