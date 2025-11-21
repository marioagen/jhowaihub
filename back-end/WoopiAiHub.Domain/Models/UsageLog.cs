using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class UsageLog : BaseEntity
    {
        [Column("NameTenant", TypeName = "varchar(150)")]
        public string NameTenant { get; private set; } = string.Empty;

        [Column("PeriodStart", TypeName = "datetime2")]
        public DateTime PeriodStart { get; private set; }

        [Column("PeriodEnd", TypeName = "datetime")]
        public DateTime PeriodEnd { get; private set; }

        [Column("TotalUsage", TypeName = "int")]
        public int TotalUsage { get; private set; }

        [Column("NameType", TypeName = "varchar(100)")]
        public string NameType { get; private set; } = string.Empty;

        public UsageLog(int id,
                        DateTime created,
                        string nameTenant,
                        DateTime periodStart,
                        DateTime periodEnd,
                        int totalUsage,
                        string nameType)
            : base(id, created)
        {
            NameTenant = nameTenant;
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
            TotalUsage = totalUsage;
            NameType = nameType;
        }
    }

}
