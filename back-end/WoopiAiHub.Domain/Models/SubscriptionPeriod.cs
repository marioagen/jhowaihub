using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class SubscriptionPeriod : BaseEntity
    {
        [Column("PeriodStart", TypeName = "datetime")]
        public DateTime PeriodStart { get; private set; }

        [Column("PeriodEnd", TypeName = "datetime")]
        public DateTime PeriodEnd { get; private set; }

        [Column("IsProcessed")]
        public bool IsProcessed { get; private set; }

        public SubscriptionPeriod(DateTime periodStart, DateTime periodEnd, bool isProcessed) : base(0, DateTime.Now)
        {
            PeriodStart = periodStart;
            PeriodEnd = periodEnd;
            IsProcessed = isProcessed;
        }

        public void SetProcessed()
        {
            IsProcessed = true;
        }

        private SubscriptionPeriod() : base(0, DateTime.Now) { }
    }
}
