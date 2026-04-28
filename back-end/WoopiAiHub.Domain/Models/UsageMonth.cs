using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class UsageMonth : BaseEntity
    {
        [Column("UsageTypeId", TypeName = "int")]
        public int UsageTypeId { get; private set; }

        [Column("Total", TypeName = "int")]
        public int Total { get; private set; }

        [Column("ModelEmbeddingId", TypeName = "int")]
        public int? ModelEmbeddingId { get; private set; }

        [Column("UserId", TypeName = "uniqueidentifier")]
        public Guid UserId { get; private set; }

        [Column("WorkflowId", TypeName = "int")]
        public int? WorkflowId { get; private set; }

        public virtual UsageType? UsageType { get; set; }
        public virtual ModelEmbedding? ModelEmbedding { get; set; }
        public virtual User? User { get; set; }
        public virtual Workflow? Workflow { get; set; }

        public UsageMonth(
            int id,
            DateTime created,
            int usageTypeId,
            int total,
            int? modelEmbeddingId,
            Guid userId,
            int? workflowId = null
        ) : base(id, created)
        {
            UsageTypeId = usageTypeId;
            Total = total;
            ModelEmbeddingId = modelEmbeddingId;
            UserId = userId;
            WorkflowId = workflowId;
        }
    }
}
