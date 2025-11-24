using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class UsageDaily : BaseEntity
    {
        [Column("UserId", TypeName = "uniqueidentifier")]
        public Guid UserId { get; private set; }

        [Column("UsageTypeId", TypeName = "int")]
        public int UsageTypeId { get; private set; }

        [Column("UsageCount", TypeName = "int")]
        public int UsageCount { get; private set; }

        [Column("Processed", TypeName = "bit")]
        public bool Processed { get; private set; }

        [Column("ModelEmbeddingId", TypeName = "int")]
        public int ModelEmbeddingId { get; private set; }

        public virtual UsageType? UsageType { get; set; }
        public virtual ModelEmbedding? ModelEmbedding { get; set; }
        public virtual User? User { get; set; }

        public UsageDaily(int id,
                          DateTime created,
                          Guid userId,
                          int usageTypeId,
                          int usageCount,
                          bool processed,
                          int modelEmbeddingId)
            : base(id, created)
        {
            UserId = userId;
            UsageTypeId = usageTypeId;
            UsageCount = usageCount;
            Processed = processed;
            ModelEmbeddingId = modelEmbeddingId;
        }
    }
}
