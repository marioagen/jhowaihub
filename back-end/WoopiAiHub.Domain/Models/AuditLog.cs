using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class AuditLog : BaseEntity
    {
        [Column("TableName", TypeName = "varchar(255)")]
        public string TableName { get; private set; } = string.Empty;

        [Column("UserId")]
        public Guid UserId { get; private set; }

        [Column("Action", TypeName = "varchar(max)")]
        public string Action { get; private set; } = string.Empty;


        public virtual User? User { get; set; }

        public AuditLog(int id, DateTime created, string tableName, Guid userId, string action)
            : base(id, created)
        {
            TableName = tableName;
            UserId = userId;
            Action = action;
        }
    }
}
