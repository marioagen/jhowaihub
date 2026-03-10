namespace WoopiAiHub.Domain.Models
{
    public class DocumentBatch : BaseEntity
    {
        public ICollection<Card> Cards { get; set; } = [];

        /// <summary>
        /// Use to EF context
        /// </summary>
        public DocumentBatch(int id, DateTime created) : base(id, created) { }
    }
}
