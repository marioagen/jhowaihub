using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class DocumentBatch : BaseEntity
    {
        [Column("CardId", TypeName = "int")]
        public int CardId { get; private set; }

        public virtual Card? Card { get; set; }

        public DocumentBatch(int id, DateTime created, int cardId)
            : base(id, created)
        {
            CardId = cardId;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private DocumentBatch(int id, DateTime created) : base(id, created) { }
    }
}
