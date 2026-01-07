using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class UsageUnit: BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; }
        [Column("UsageTypeId", TypeName = "int")]
        public int? UsageTypeId { get; private set; }
        [Column("ModelEmbeddingId", TypeName = "int")]
        public int? ModelEmbeddingId { get; private set; }
        [Column("Value", TypeName = "decimal(18,7)")]
        public decimal Value { get; private set; }

        public virtual UsageType? UsageType { get; set; }
        public virtual ModelEmbedding? ModelEmbedding { get; set; }

        public UsageUnit(int id,
                         DateTime created,
                         string name,
                         int? usageTypeId,
                         int? modelEmbeddingId,
                         decimal value)
            : base(id, created)
        {
            Name = name;
            UsageTypeId = usageTypeId;
            ModelEmbeddingId = modelEmbeddingId;
            Value = value;
        }
    }
}
